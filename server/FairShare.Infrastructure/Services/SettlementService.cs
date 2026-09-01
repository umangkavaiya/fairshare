using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FairShare.Application.DTOs.Common;
using FairShare.Application.DTOs.Settlements;
using FairShare.Application.Interfaces;
using FairShare.Domain.Entities;
using FairShare.Infrastructure.Data;

namespace FairShare.Infrastructure.Services;

public class SettlementService : ISettlementService
{
    private readonly ApplicationDbContext _db;
    private readonly IDebtSimplificationService _debtSimplifier;

    public SettlementService(ApplicationDbContext db, IDebtSimplificationService debtSimplifier)
    {
        _db = db;
        _debtSimplifier = debtSimplifier;
    }

    public async Task<List<BalanceDto>> GetBalancesAsync(Guid groupId, Guid userId)
    {
        await RequireMembershipAsync(groupId, userId);
        var netBalances = await ComputeNetBalancesAsync(groupId);

        var members = await _db.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsActive)
            .Select(gm => new { gm.UserId, gm.User.DisplayName })
            .ToListAsync();

        return members.Select(m => new BalanceDto
        {
            UserId = m.UserId,
            DisplayName = m.DisplayName,
            NetBalance = netBalances.GetValueOrDefault(m.UserId, 0m)
        }).ToList();
    }

    public async Task<List<SettlementSuggestionDto>> GetSuggestionsAsync(Guid groupId, Guid userId)
    {
        await RequireMembershipAsync(groupId, userId);
        var netBalances = await ComputeNetBalancesAsync(groupId);
        var transactions = _debtSimplifier.Simplify(netBalances);

        var displayNames = await _db.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsActive)
            .ToDictionaryAsync(gm => gm.UserId, gm => gm.User.DisplayName);

        return transactions.Select(t => new SettlementSuggestionDto
        {
            FromUserId = t.FromUserId,
            FromDisplayName = displayNames.GetValueOrDefault(t.FromUserId, "Unknown"),
            ToUserId = t.ToUserId,
            ToDisplayName = displayNames.GetValueOrDefault(t.ToUserId, "Unknown"),
            Amount = t.Amount
        }).ToList();
    }

    public async Task<PagedResult<SettlementResponseDto>> GetHistoryAsync(Guid groupId, Guid userId, int page, int pageSize)
    {
        await RequireMembershipAsync(groupId, userId);

        var query = _db.Settlements.Where(s => s.GroupId == groupId);
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(s => s.Payer)
            .Include(s => s.Payee)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return new PagedResult<SettlementResponseDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<SettlementResponseDto> CreateSettlementAsync(Guid groupId, Guid userId, CreateSettlementDto dto)
    {
        await RequireMembershipAsync(groupId, userId);
        await RequireMembershipAsync(groupId, dto.PayerUserId);
        await RequireMembershipAsync(groupId, dto.PayeeUserId);

        var settlement = new Settlement
        {
            GroupId = groupId,
            PayerUserId = dto.PayerUserId,
            PayeeUserId = dto.PayeeUserId,
            Amount = dto.Amount,
            CurrencyCode = dto.CurrencyCode,
            Note = dto.Note,
            SettledAt = dto.SettledAt,
            CreatedByUserId = userId,
            Status = SettlementStatus.Pending
        };

        _db.Settlements.Add(settlement);
        await _db.SaveChangesAsync();

        return await LoadAndMapAsync(settlement.Id);
    }

    public async Task<SettlementResponseDto> ConfirmSettlementAsync(Guid settlementId, Guid userId)
    {
        var settlement = await _db.Settlements.FirstOrDefaultAsync(s => s.Id == settlementId)
            ?? throw new KeyNotFoundException("Settlement not found.");

        if (settlement.Status == SettlementStatus.Confirmed)
            return await LoadAndMapAsync(settlement.Id);

        // Only the counterparty to the transaction (not the creator themselves) can confirm —
        // this is what makes the confirmation meaningful rather than a rubber stamp.
        var counterpartyId = settlement.CreatedByUserId == settlement.PayerUserId
            ? settlement.PayeeUserId
            : settlement.PayerUserId;

        if (userId != counterpartyId)
            throw new UnauthorizedAccessException("Only the other party to this settlement can confirm it.");

        settlement.Status = SettlementStatus.Confirmed;
        settlement.ConfirmedByUserId = userId;
        settlement.ConfirmedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await LoadAndMapAsync(settlement.Id);
    }

    // ---- helpers ----

    private async Task<Dictionary<Guid, decimal>> ComputeNetBalancesAsync(Guid groupId)
    {
        var balances = new Dictionary<Guid, decimal>();

        void Add(Guid userId, decimal amount) =>
            balances[userId] = balances.GetValueOrDefault(userId, 0m) + amount;

        var paidTotals = await _db.Expenses
            .Where(e => e.GroupId == groupId && !e.IsDeleted)
            .GroupBy(e => e.PaidByUserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(e => e.Amount) })
            .ToListAsync();
        foreach (var p in paidTotals) Add(p.UserId, p.Total);

        var owedTotals = await _db.ExpenseSplits
            .Where(s => s.Expense.GroupId == groupId && !s.Expense.IsDeleted)
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(s => s.AmountOwed) })
            .ToListAsync();
        foreach (var o in owedTotals) Add(o.UserId, -o.Total);

        var confirmedSettlements = await _db.Settlements
            .Where(s => s.GroupId == groupId && s.Status == SettlementStatus.Confirmed)
            .ToListAsync();
        foreach (var s in confirmedSettlements)
        {
            Add(s.PayerUserId, s.Amount);   // paying off debt increases their balance
            Add(s.PayeeUserId, -s.Amount);  // receiving payment reduces what's owed to them
        }

        return balances;
    }

    private async Task RequireMembershipAsync(Guid groupId, Guid userId)
    {
        var isMember = await _db.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this group.");
    }

    private async Task<SettlementResponseDto> LoadAndMapAsync(Guid settlementId)
    {
        var settlement = await _db.Settlements
            .Include(s => s.Payer)
            .Include(s => s.Payee)
            .FirstAsync(s => s.Id == settlementId);
        return MapToDto(settlement);
    }

    private static SettlementResponseDto MapToDto(Settlement s) => new()
    {
        Id = s.Id,
        GroupId = s.GroupId,
        PayerUserId = s.PayerUserId,
        PayerDisplayName = s.Payer.DisplayName,
        PayeeUserId = s.PayeeUserId,
        PayeeDisplayName = s.Payee.DisplayName,
        Amount = s.Amount,
        CurrencyCode = s.CurrencyCode,
        Note = s.Note,
        Status = s.Status.ToString(),
        CreatedByUserId = s.CreatedByUserId,
        SettledAt = s.SettledAt,
        CreatedAt = s.CreatedAt,
        ConfirmedAt = s.ConfirmedAt
    };
}

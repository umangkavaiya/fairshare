using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FairShare.Application.DTOs.Common;
using FairShare.Application.DTOs.Expenses;
using FairShare.Application.Exceptions;
using FairShare.Application.Interfaces;
using FairShare.Domain.Entities;
using FairShare.Infrastructure.Data;

namespace FairShare.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _db;
    private readonly ISplitCalculationService _splitCalculator;

    public ExpenseService(ApplicationDbContext db, ISplitCalculationService splitCalculator)
    {
        _db = db;
        _splitCalculator = splitCalculator;
    }

    public async Task<PagedResult<ExpenseResponseDto>> GetExpensesAsync(Guid userId, Guid? groupId, int page, int pageSize)
    {
        IQueryable<Expense> query = _db.Expenses.Where(e => !e.IsDeleted);

        if (groupId.HasValue)
        {
            await RequireMembershipAsync(groupId.Value, userId);
            query = query.Where(e => e.GroupId == groupId);
        }
        else
        {
            // Personal expenses only — never leak another user's private expenses
            query = query.Where(e => e.GroupId == null && e.CreatedByUserId == userId);
        }

        var totalCount = await query.CountAsync();

        var expenses = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => e.Id)
            .ToListAsync();

        var items = new List<ExpenseResponseDto>();
        foreach (var id in expenses)
            items.Add(await MapToDto(id));

        return new PagedResult<ExpenseResponseDto> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<ExpenseResponseDto> GetExpenseByIdAsync(Guid expenseId, Guid userId)
    {
        var expense = await GetActiveExpenseAsync(expenseId);
        await AuthorizeAccessAsync(expense, userId);
        return await MapToDto(expenseId);
    }

    public async Task<ExpenseResponseDto> CreateExpenseAsync(Guid userId, CreateExpenseDto dto)
    {
        if (dto.GroupId.HasValue)
            await RequireMembershipAsync(dto.GroupId.Value, userId);

        var splits = CalculateSplits(dto.SplitType, dto.Amount, dto.Participants);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var expense = new Expense
        {
            GroupId = dto.GroupId,
            PaidByUserId = dto.PaidByUserId,
            CreatedByUserId = userId,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            Amount = dto.Amount,
            CurrencyCode = dto.CurrencyCode,
            SplitType = Enum.Parse<SplitType>(dto.SplitType),
            ExpenseDate = dto.ExpenseDate
        };
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync(); // need expense.Id for the splits' FK

        foreach (var (userIdSplit, amount, percentage) in splits)
        {
            _db.ExpenseSplits.Add(new ExpenseSplit
            {
                ExpenseId = expense.Id,
                UserId = userIdSplit,
                AmountOwed = amount,
                Percentage = percentage
            });
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await MapToDto(expense.Id);
    }

    public async Task<ExpenseResponseDto> UpdateExpenseAsync(Guid expenseId, Guid userId, UpdateExpenseDto dto)
    {
        var expense = await GetActiveExpenseAsync(expenseId);
        await AuthorizeAccessAsync(expense, userId);

        _db.Entry(expense).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(dto.RowVersion);

        var splits = CalculateSplits(dto.SplitType, dto.Amount, dto.Participants);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            expense.Description = dto.Description;
            expense.Amount = dto.Amount;
            expense.CategoryId = dto.CategoryId;
            expense.SplitType = Enum.Parse<SplitType>(dto.SplitType);
            expense.ExpenseDate = dto.ExpenseDate;
            expense.UpdatedAt = DateTime.UtcNow;

            var oldSplits = await _db.ExpenseSplits.Where(s => s.ExpenseId == expenseId).ToListAsync();
            _db.ExpenseSplits.RemoveRange(oldSplits);

            foreach (var (splitUserId, amount, percentage) in splits)
            {
                _db.ExpenseSplits.Add(new ExpenseSplit
                {
                    ExpenseId = expenseId,
                    UserId = splitUserId,
                    AmountOwed = amount,
                    Percentage = percentage
                });
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            throw await BuildConcurrencyExceptionAsync(ex);
        }

        return await MapToDto(expenseId);
    }

    public async Task<ExpenseResponseDto> PatchExpenseAsync(Guid expenseId, Guid userId, PatchExpenseDto dto)
    {
        var expense = await GetActiveExpenseAsync(expenseId);
        await AuthorizeAccessAsync(expense, userId);

        _db.Entry(expense).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(dto.RowVersion);

        if (dto.Description is not null) expense.Description = dto.Description;
        if (dto.CategoryId is not null) expense.CategoryId = dto.CategoryId;
        if (dto.ExpenseDate is not null) expense.ExpenseDate = dto.ExpenseDate.Value;
        expense.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw await BuildConcurrencyExceptionAsync(ex);
        }

        return await MapToDto(expenseId);
    }

    public async Task DeleteExpenseAsync(Guid expenseId, Guid userId)
    {
        var expense = await GetActiveExpenseAsync(expenseId);
        await AuthorizeAccessAsync(expense, userId);

        expense.IsDeleted = true;
        expense.DeletedAt = DateTime.UtcNow;
        expense.DeletedBy = userId;
        await _db.SaveChangesAsync();
    }

    public async Task<ExpenseResponseDto> RestoreExpenseAsync(Guid expenseId, Guid userId)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && e.IsDeleted)
            ?? throw new KeyNotFoundException("Deleted expense not found.");

        await AuthorizeAccessAsync(expense, userId, requireAdminIfGroup: true);

        expense.IsDeleted = false;
        expense.DeletedAt = null;
        expense.DeletedBy = null;
        await _db.SaveChangesAsync();

        return await MapToDto(expenseId);
    }

    // ---- helpers ----

    private List<(Guid UserId, decimal Amount, decimal? Percentage)> CalculateSplits(string splitType, decimal amount, List<ExpenseParticipantInputDto> participants)
    {
        return splitType switch
        {
            "Equal" => _splitCalculator.CalculateEqualSplit(amount, participants.Select(p => p.UserId).ToList())
                .Select(r => (r.UserId, r.Amount, (decimal?)null)).ToList(),

            "Exact" => _splitCalculator.CalculateExactSplit(amount, participants.ToDictionary(p => p.UserId, p => p.Amount ?? 0m))
                .Select(r => (r.UserId, r.Amount, (decimal?)null)).ToList(),

            "Percentage" => _splitCalculator.CalculatePercentageSplit(amount, participants.ToDictionary(p => p.UserId, p => p.Percentage ?? 0m))
                .Select(r => (r.UserId, r.Amount, (decimal?)r.Percentage)).ToList(),

            _ => throw new ArgumentException($"Unknown split type: {splitType}")
        };
    }

    private async Task<Expense> GetActiveExpenseAsync(Guid expenseId) =>
        await _db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && !e.IsDeleted)
            ?? throw new KeyNotFoundException("Expense not found.");

    private async Task RequireMembershipAsync(Guid groupId, Guid userId)
    {
        var isMember = await _db.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this group.");
    }

    private async Task AuthorizeAccessAsync(Expense expense, Guid userId, bool requireAdminIfGroup = false)
    {
        if (expense.GroupId is null)
        {
            if (expense.CreatedByUserId != userId)
                throw new UnauthorizedAccessException("You do not have access to this expense.");
            return;
        }

        var membership = await _db.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == expense.GroupId && gm.UserId == userId && gm.IsActive)
            ?? throw new UnauthorizedAccessException("You are not a member of this group.");

        var isOwnerOrAdmin = expense.CreatedByUserId == userId || membership.Role == GroupRole.Admin;
        if (requireAdminIfGroup && membership.Role != GroupRole.Admin)
            throw new UnauthorizedAccessException("Only group admins can restore a deleted expense.");
        if (!isOwnerOrAdmin)
            throw new UnauthorizedAccessException("Only the expense creator or a group admin can modify this expense.");
    }

    private async Task<ConcurrencyConflictException> BuildConcurrencyExceptionAsync(DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync();
        var currentRowVersion = databaseValues?["RowVersion"] as byte[] ?? Array.Empty<byte>();
        return new ConcurrencyConflictException(Convert.ToBase64String(currentRowVersion));
    }

    private async Task<ExpenseResponseDto> MapToDto(Guid expenseId)
    {
        var expense = await _db.Expenses
            .Include(e => e.PaidBy)
            .Include(e => e.Category)
            .Include(e => e.Splits).ThenInclude(s => s.User)
            .FirstAsync(e => e.Id == expenseId);

        return new ExpenseResponseDto
        {
            Id = expense.Id,
            GroupId = expense.GroupId,
            Description = expense.Description,
            Amount = expense.Amount,
            CurrencyCode = expense.CurrencyCode,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category?.Name,
            SplitType = expense.SplitType.ToString(),
            ExpenseDate = expense.ExpenseDate,
            PaidByUserId = expense.PaidByUserId,
            PaidByDisplayName = expense.PaidBy.DisplayName,
            CreatedByUserId = expense.CreatedByUserId,
            CreatedAt = expense.CreatedAt,
            UpdatedAt = expense.UpdatedAt,
            RowVersion = Convert.ToBase64String(expense.RowVersion),
            Splits = expense.Splits.Select(s => new ExpenseSplitResponseDto
            {
                UserId = s.UserId,
                DisplayName = s.User.DisplayName,
                AmountOwed = s.AmountOwed,
                Percentage = s.Percentage
            }).ToList()
        };
    }
}


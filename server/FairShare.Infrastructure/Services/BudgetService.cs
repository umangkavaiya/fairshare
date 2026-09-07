using System;
using System.Collections.Generic;
using System.Text;
using FairShare.Application.DTOs.Budgets;
using FairShare.Application.Exceptions;
using FairShare.Application.Interfaces;
using FairShare.Domain.Entities;
using FairShare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FairShare.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly ApplicationDbContext _db;

    public BudgetService(ApplicationDbContext db) => _db = db;

    public async Task<List<BudgetResponseDto>> GetBudgetsAsync(Guid userId)
    {
        return await _db.Budgets
            .Where(b => b.UserId == userId && b.IsActive)
            .Include(b => b.Category)
            .Select(b => ToDto(b))
            .ToListAsync();
    }

    public async Task<BudgetResponseDto?> GetBudgetByIdAsync(Guid userId, Guid budgetId)
    {
        var budget = await _db.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);

        return budget is null ? null : ToDto(budget);
    }

    public async Task<BudgetResponseDto> CreateBudgetAsync(Guid userId, CreateBudgetDto dto)
    {
        // reactivate-don't-duplicate, same lesson as GroupMember re-invite (Phase 5)
        var existing = await _db.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == dto.CategoryId);

        if (existing is not null)
        {
            if (existing.IsActive)
                throw new InvalidOperationException("An active budget already exists for this category.");

            existing.IsActive = true;
            existing.MonthlyLimit = dto.MonthlyLimit;
            existing.CurrencyCode = dto.CurrencyCode;
            existing.RolloverEnabled = dto.RolloverEnabled;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(existing);
        }

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            MonthlyLimit = dto.MonthlyLimit,
            CurrencyCode = dto.CurrencyCode,
            RolloverEnabled = dto.RolloverEnabled
        };

        _db.Budgets.Add(budget);
        await _db.SaveChangesAsync();
        await _db.Entry(budget).Reference(b => b.Category).LoadAsync();
        return ToDto(budget);
    }

    public async Task<BudgetResponseDto> UpdateBudgetAsync(Guid userId, Guid budgetId, UpdateBudgetDto dto)
    {
        var budget = await _db.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId)
            ?? throw new KeyNotFoundException("Budget not found.");

        budget.MonthlyLimit = dto.MonthlyLimit;
        budget.RolloverEnabled = dto.RolloverEnabled;
        budget.UpdatedAt = DateTime.UtcNow;

        _db.Entry(budget).Property(b => b.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var currentValue = await _db.Budgets
                .Where(b => b.Id == budgetId)
                .Select(b => b.RowVersion)
                .FirstOrDefaultAsync();

            throw new ConcurrencyConflictException(Convert.ToBase64String(currentValue ?? Array.Empty<byte>()));
        }

        return ToDto(budget);
    }

    public async Task DeactivateBudgetAsync(Guid userId, Guid budgetId)
    {
        var budget = await _db.Budgets
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId)
            ?? throw new KeyNotFoundException("Budget not found.");

        budget.IsActive = false;
        budget.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static BudgetResponseDto ToDto(Budget b) => new()
    {
        Id = b.Id,
        CategoryId = b.CategoryId,
        CategoryName = b.Category.Name,
        MonthlyLimit = b.MonthlyLimit,
        CurrencyCode = b.CurrencyCode,
        RolloverEnabled = b.RolloverEnabled,
        IsActive = b.IsActive,
        RowVersion = b.RowVersion,
        CreatedAt = b.CreatedAt
    };
}

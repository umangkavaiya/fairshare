using System;
using System.Collections.Generic;
using System.Text;
using FairShare.Application.DTOs.Budgets;

namespace FairShare.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetResponseDto>> GetBudgetsAsync(Guid userId);
    Task<BudgetResponseDto?> GetBudgetByIdAsync(Guid userId, Guid budgetId);
    Task<BudgetResponseDto> CreateBudgetAsync(Guid userId, CreateBudgetDto dto);
    Task<BudgetResponseDto> UpdateBudgetAsync(Guid userId, Guid budgetId, UpdateBudgetDto dto);
    Task DeactivateBudgetAsync(Guid userId, Guid budgetId);
}

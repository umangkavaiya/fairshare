using System;
using System.Collections.Generic;
using System.Text;
using FairShare.Application.DTOs.Common;
using FairShare.Application.DTOs.Expenses;

namespace FairShare.Application.Interfaces;

public interface IExpenseService
{
    Task<PagedResult<ExpenseResponseDto>> GetExpensesAsync(Guid userId, Guid? groupId, int page, int pageSize);
    Task<ExpenseResponseDto> GetExpenseByIdAsync(Guid expenseId, Guid userId);
    Task<ExpenseResponseDto> CreateExpenseAsync(Guid userId, CreateExpenseDto dto);
    Task<ExpenseResponseDto> UpdateExpenseAsync(Guid expenseId, Guid userId, UpdateExpenseDto dto);
    Task<ExpenseResponseDto> PatchExpenseAsync(Guid expenseId, Guid userId, PatchExpenseDto dto);
    Task DeleteExpenseAsync(Guid expenseId, Guid userId);
    Task<ExpenseResponseDto> RestoreExpenseAsync(Guid expenseId, Guid userId);
}

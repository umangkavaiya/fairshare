using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FairShare.Api.Extensions;
using FairShare.Api.Filters;
using FairShare.Application.DTOs.Expenses;
using FairShare.Application.Interfaces;

namespace FairShare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService) => _expenseService = expenseService;

    [HttpGet]
    public async Task<IActionResult> GetExpenses([FromQuery] Guid? groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _expenseService.GetExpensesAsync(User.GetUserId(), groupId, page, pageSize));

    [HttpGet("{expenseId:guid}")]
    public async Task<IActionResult> GetExpense(Guid expenseId)
        => Ok(await _expenseService.GetExpenseByIdAsync(expenseId, User.GetUserId()));

    [HttpPost]
    [IdempotencyFilter]
    public async Task<IActionResult> CreateExpense(CreateExpenseDto dto)
    {
        var expense = await _expenseService.CreateExpenseAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(GetExpense), new { expenseId = expense.Id }, expense);
    }

    [HttpPut("{expenseId:guid}")]
    public async Task<IActionResult> UpdateExpense(Guid expenseId, UpdateExpenseDto dto)
        => Ok(await _expenseService.UpdateExpenseAsync(expenseId, User.GetUserId(), dto));

    [HttpPatch("{expenseId:guid}")]
    public async Task<IActionResult> PatchExpense(Guid expenseId, PatchExpenseDto dto)
        => Ok(await _expenseService.PatchExpenseAsync(expenseId, User.GetUserId(), dto));

    [HttpDelete("{expenseId:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid expenseId)
    {
        await _expenseService.DeleteExpenseAsync(expenseId, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{expenseId:guid}/restore")]
    public async Task<IActionResult> RestoreExpense(Guid expenseId)
        => Ok(await _expenseService.RestoreExpenseAsync(expenseId, User.GetUserId()));
}

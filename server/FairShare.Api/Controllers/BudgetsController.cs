using FairShare.Api.Extensions;
using FairShare.Api.Filters;
using FairShare.Application.DTOs.Budgets;
using FairShare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FairShare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService) => _budgetService = budgetService;

    [HttpGet]
    public async Task<IActionResult> GetBudgets()
    {
        var budgets = await _budgetService.GetBudgetsAsync(User.GetUserId());
        return Ok(budgets);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBudget(Guid id)
    {
        var budget = await _budgetService.GetBudgetByIdAsync(User.GetUserId(), id);
        return budget is null ? NotFound() : Ok(budget);
    }

    [HttpPost]
    [IdempotencyFilter]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto dto)
    {
        var budget = await _budgetService.CreateBudgetAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(GetBudget), new { id = budget.Id }, budget);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateBudgetDto dto)
    {
        var budget = await _budgetService.UpdateBudgetAsync(User.GetUserId(), id, dto);
        return Ok(budget);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateBudget(Guid id)
    {
        await _budgetService.DeactivateBudgetAsync(User.GetUserId(), id);
        return NoContent();
    }
}
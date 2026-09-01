using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FairShare.Api.Extensions;
using FairShare.Api.Filters;
using FairShare.Application.DTOs.Settlements;
using FairShare.Application.Interfaces;

namespace FairShare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:guid}")]
public class SettlementsController : ControllerBase
{
    private readonly ISettlementService _settlementService;

    public SettlementsController(ISettlementService settlementService) => _settlementService = settlementService;

    [HttpGet("balances")]
    public async Task<IActionResult> GetBalances(Guid groupId)
        => Ok(await _settlementService.GetBalancesAsync(groupId, User.GetUserId()));

    [HttpGet("settlements/suggestions")]
    public async Task<IActionResult> GetSuggestions(Guid groupId)
        => Ok(await _settlementService.GetSuggestionsAsync(groupId, User.GetUserId()));

    [HttpGet("settlements")]
    public async Task<IActionResult> GetHistory(Guid groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _settlementService.GetHistoryAsync(groupId, User.GetUserId(), page, pageSize));

    [HttpPost("settlements")]
    [IdempotencyFilter]
    public async Task<IActionResult> CreateSettlement(Guid groupId, CreateSettlementDto dto)
        => Ok(await _settlementService.CreateSettlementAsync(groupId, User.GetUserId(), dto));

    [HttpPost("settlements/{settlementId:guid}/confirm")]
    public async Task<IActionResult> ConfirmSettlement(Guid groupId, Guid settlementId)
        => Ok(await _settlementService.ConfirmSettlementAsync(settlementId, User.GetUserId()));
}
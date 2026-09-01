using System;
using System.Collections.Generic;
using System.Text;
using FairShare.Application.DTOs.Common;
using FairShare.Application.DTOs.Settlements;

namespace FairShare.Application.Interfaces;

public interface ISettlementService
{
    Task<List<BalanceDto>> GetBalancesAsync(Guid groupId, Guid userId);
    Task<List<SettlementSuggestionDto>> GetSuggestionsAsync(Guid groupId, Guid userId);
    Task<PagedResult<SettlementResponseDto>> GetHistoryAsync(Guid groupId, Guid userId, int page, int pageSize);
    Task<SettlementResponseDto> CreateSettlementAsync(Guid groupId, Guid userId, CreateSettlementDto dto);
    Task<SettlementResponseDto> ConfirmSettlementAsync(Guid settlementId, Guid userId);
}

using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Settlements;

public class SettlementSuggestionDto
{
    public Guid FromUserId { get; set; }
    public string FromDisplayName { get; set; } = string.Empty;
    public Guid ToUserId { get; set; }
    public string ToDisplayName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
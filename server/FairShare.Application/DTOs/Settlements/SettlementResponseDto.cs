using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Settlements;

public class SettlementResponseDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PayerUserId { get; set; }
    public string PayerDisplayName { get; set; } = string.Empty;
    public Guid PayeeUserId { get; set; }
    public string PayeeDisplayName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTime SettledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}

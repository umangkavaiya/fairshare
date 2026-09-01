using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Settlements;

public class CreateSettlementDto
{
    public Guid PayerUserId { get; set; }
    public Guid PayeeUserId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Note { get; set; }
    public DateTime SettledAt { get; set; }
}
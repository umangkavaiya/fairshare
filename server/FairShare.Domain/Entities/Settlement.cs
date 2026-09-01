using System;
using System.Collections.Generic;
using System.Text;


namespace FairShare.Domain.Entities;

public enum SettlementStatus : byte { Pending = 1, Confirmed = 2 }

public class Settlement : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid PayerUserId { get; set; }
    public Guid PayeeUserId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Note { get; set; }
    public SettlementStatus Status { get; set; } = SettlementStatus.Pending;
    public Guid CreatedByUserId { get; set; }
    public DateTime SettledAt { get; set; }
    public Guid? ConfirmedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public Group Group { get; set; } = null!;
    public AppUser Payer { get; set; } = null!;
    public AppUser Payee { get; set; } = null!;
}

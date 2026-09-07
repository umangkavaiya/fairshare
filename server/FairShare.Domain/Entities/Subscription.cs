using System;
using System.Collections.Generic;
using System.Text;
using System;

namespace FairShare.Domain.Entities;

public enum BillingCycle : byte { Weekly = 1, Monthly = 2, Quarterly = 3, Yearly = 4 }

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public int? CategoryId { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public DateTime NextBillingDate { get; set; }
    public bool AutoGenerateExpense { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public AppUser User { get; set; } = null!;
    public ExpenseCategory? Category { get; set; }
}

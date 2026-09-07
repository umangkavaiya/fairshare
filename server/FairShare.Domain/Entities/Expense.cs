using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FairShare.Domain.Entities;

public enum SplitType : byte { Equal = 1, Exact = 2, Percentage = 3, ItemWise = 4 }

public class Expense : BaseEntity
{
    public Guid? GroupId { get; set; }
    public Guid PaidByUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public int? CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public SplitType SplitType { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public Group? Group { get; set; }
    public AppUser PaidBy { get; set; } = null!;
    public ExpenseCategory? Category { get; set; }
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
}
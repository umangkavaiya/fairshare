using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;

namespace FairShare.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public int CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool RolloverEnabled { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public AppUser User { get; set; } = null!;
    public ExpenseCategory Category { get; set; } = null!;
    public ICollection<BudgetPeriod> Periods { get; set; } = new List<BudgetPeriod>();
}

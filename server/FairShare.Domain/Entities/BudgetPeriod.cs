using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Domain.Entities;

// Written once, when a rollover-enabled budget's month closes. Never touched
// for non-rollover budgets — "spent this month" is computed live for those.
public class BudgetPeriod : BaseEntity
{
    public Guid BudgetId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal CarriedInAmount { get; set; }
    public decimal EffectiveLimit { get; set; }
    public decimal SpentAmount { get; set; }

    public Budget Budget { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FairShare.Domain.Entities;

public class ExpenseSplit : BaseEntity
{
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }
    public decimal AmountOwed { get; set; }
    public decimal? Percentage { get; set; }

    public Expense Expense { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
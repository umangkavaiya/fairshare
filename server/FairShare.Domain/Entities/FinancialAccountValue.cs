using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Domain.Entities;

// Append-only ledger — never updated in place, only ever inserted, so net worth
// can be charted over time. Value is always a positive magnitude; direction
// comes from the parent FinancialAccount.Type (Asset adds, Liability subtracts).
public class FinancialAccountValue : BaseEntity
{
    public Guid FinancialAccountId { get; set; }
    public decimal Value { get; set; }
    public DateTime AsOfDate { get; set; }

    public FinancialAccount FinancialAccount { get; set; } = null!;
}

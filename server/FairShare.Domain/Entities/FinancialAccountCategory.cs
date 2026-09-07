using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Domain.Entities;

public enum FinancialAccountType : byte { Asset = 1, Liability = 2 }

public class FinancialAccountCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public FinancialAccountType? AppliesToType { get; set; }
}

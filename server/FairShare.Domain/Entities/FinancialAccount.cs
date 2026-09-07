using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Domain.Entities;

public class FinancialAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public FinancialAccountType Type { get; set; }
    public int CategoryId { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public AppUser User { get; set; } = null!;
    public FinancialAccountCategory Category { get; set; } = null!;
    public ICollection<FinancialAccountValue> Values { get; set; } = new List<FinancialAccountValue>();
}

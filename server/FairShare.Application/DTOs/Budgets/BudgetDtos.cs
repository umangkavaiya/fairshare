using System;
using System.Collections.Generic;
using System.Text;
namespace FairShare.Application.DTOs.Budgets;

public class BudgetResponseDto
{
    public Guid Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal MonthlyLimit { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public bool RolloverEnabled { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class CreateBudgetDto
{
    public int CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public bool RolloverEnabled { get; set; }
}

public class UpdateBudgetDto
{
    public decimal MonthlyLimit { get; set; }
    public bool RolloverEnabled { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}

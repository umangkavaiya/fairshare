using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Expenses;

public class ExpenseSplitResponseDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal AmountOwed { get; set; }
    public decimal? Percentage { get; set; }
}

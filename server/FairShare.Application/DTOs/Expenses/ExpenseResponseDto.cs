using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Expenses;

public class ExpenseResponseDto
{
    public Guid Id { get; set; }
    public Guid? GroupId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string SplitType { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public Guid PaidByUserId { get; set; }
    public string PaidByDisplayName { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string RowVersion { get; set; } = string.Empty; // base64
    public List<ExpenseSplitResponseDto> Splits { get; set; } = new();
}

using System;
using System.Collections.Generic;
using System.Text;


namespace FairShare.Application.DTOs.Expenses;

public class CreateExpenseDto
{
    public Guid? GroupId { get; set; }
    public Guid PaidByUserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public int? CategoryId { get; set; }
    public string SplitType { get; set; } = "Equal"; // Equal | Exact | Percentage
    public DateTime ExpenseDate { get; set; }
    public List<ExpenseParticipantInputDto> Participants { get; set; } = new();
}
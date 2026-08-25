using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Expenses;

// PUT — full replace, including a full split recalculation
public class UpdateExpenseDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
    public string SplitType { get; set; } = "Equal";
    public DateTime ExpenseDate { get; set; }
    public List<ExpenseParticipantInputDto> Participants { get; set; } = new();
    public string RowVersion { get; set; } = string.Empty; // base64 — the version the client last saw
}
using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Expenses;

// PATCH — metadata only. Amount/splits are structurally a PUT-level change.
public class PatchExpenseDto
{
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public DateTime? ExpenseDate { get; set; }
    public string RowVersion { get; set; } = string.Empty; // required even for metadata-only edits
}
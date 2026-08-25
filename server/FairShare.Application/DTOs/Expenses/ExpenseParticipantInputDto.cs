using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.DTOs.Expenses;

public class ExpenseParticipantInputDto
{
    public Guid UserId { get; set; }
    public decimal? Amount { get; set; }      // used for Exact split
    public decimal? Percentage { get; set; }  // used for Percentage split
}
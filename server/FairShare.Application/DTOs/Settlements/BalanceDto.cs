using System;
using System.Collections.Generic;
using System.Text;


namespace FairShare.Application.DTOs.Settlements;

public class BalanceDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal NetBalance { get; set; } // positive = owed to them, negative = they owe
}

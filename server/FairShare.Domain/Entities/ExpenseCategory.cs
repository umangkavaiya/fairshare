using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FairShare.Domain.Entities;

public class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
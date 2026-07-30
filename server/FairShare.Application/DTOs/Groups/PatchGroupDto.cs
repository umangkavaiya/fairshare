using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FairShare.Application.DTOs.Groups;

// PATCH — only supplied (non-null) fields are applied
public class PatchGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? DefaultCurrency { get; set; }
}
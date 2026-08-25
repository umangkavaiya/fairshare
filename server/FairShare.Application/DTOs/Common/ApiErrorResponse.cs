using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FairShare.Application.DTOs.Common;

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string? CurrentVersion { get; set; }
    public List<string> Errors { get; set; } = new();
}
using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Domain.Entities;

public class IdempotencyKey
{
    public string Key { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public int ResponseStatusCode { get; set; }
    public string ResponseBody { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}

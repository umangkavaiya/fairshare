using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FairShare.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }

    public AppUser User { get; set; } = null!;

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}
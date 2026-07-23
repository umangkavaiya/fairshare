using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FairShare.Application.DTOs.Auth;

// What the service layer returns internally — includes the raw refresh token
// so the controller can set it as a cookie. Never serialized directly to the client.
public class AuthResultDto
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserDto? User { get; set; }
}
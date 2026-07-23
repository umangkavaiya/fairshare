using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// The public-facing shape — deliberately excludes the refresh token, which only ever travels as a cookie
namespace FairShare.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}
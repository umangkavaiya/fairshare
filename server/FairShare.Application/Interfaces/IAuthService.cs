using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FairShare.Application.DTOs.Auth;

namespace FairShare.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);
    Task<AuthResultDto> RefreshAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
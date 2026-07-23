using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FairShare.Application.DTOs.Auth;
using FairShare.Application.Interfaces;
using FairShare.Domain.Entities;
using FairShare.Infrastructure.Data;

namespace FairShare.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _db;

    public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, ApplicationDbContext db)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _db = db;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Fail("An account with this email already exists.");

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Fail(string.Join(" ", result.Errors.Select(e => e.Description)));

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Fail("Invalid email or password."); // generic — never reveal which field was wrong

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Fail("Invalid email or password.");

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResultDto> RefreshAsync(string refreshToken)
    {
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (stored is null || !stored.IsActive)
            return Fail("Invalid or expired refresh token.");

        // Rotation: revoke the old token, issue a brand new pair
        stored.IsRevoked = true;

        var result = await IssueTokensAsync(stored.User);
        stored.ReplacedByToken = result.RefreshToken;
        await _db.SaveChangesAsync();

        return result;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (stored is not null)
        {
            stored.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    private async Task<AuthResultDto> IssueTokensAsync(AppUser user)
    {
        var (accessToken, accessExpiry) = _tokenService.GenerateAccessToken(user);
        var (refreshToken, refreshExpiry) = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = refreshExpiry
        });
        await _db.SaveChangesAsync();

        return new AuthResultDto
        {
            Succeeded = true,
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpiry,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiry,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                DefaultCurrency = user.DefaultCurrency
            }
        };
    }

    private static AuthResultDto Fail(string message) => new() { Succeeded = false, ErrorMessage = message };
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FairShare.Application.DTOs.Auth;
using FairShare.Application.Interfaces;

namespace FairShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (!result.Succeeded)
            return BadRequest(new { message = result.ErrorMessage });

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(ToResponse(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Succeeded)
            return Unauthorized(new { message = result.ErrorMessage });

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "No refresh token provided." });

        var result = await _authService.RefreshAsync(refreshToken);
        if (!result.Succeeded)
        {
            Response.Cookies.Delete("refreshToken");
            return Unauthorized(new { message = result.ErrorMessage });
        }

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(ToResponse(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.RevokeRefreshTokenAsync(refreshToken);

        Response.Cookies.Delete("refreshToken");
        return NoContent();
    }

    private void SetRefreshTokenCookie(string token, DateTime expiresAt)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Expires = expiresAt,
            SameSite = SameSiteMode.Lax,   // Lax works for localhost:4200 -> localhost:7xxx in dev
            Secure = !_env.IsDevelopment()  // relax Secure requirement only in dev over http
        });
    }

    private static AuthResponseDto ToResponse(AuthResultDto result) => new()
    {
        AccessToken = result.AccessToken,
        AccessTokenExpiresAt = result.AccessTokenExpiresAt,
        User = result.User!
    };
}
using Hekucoreapp.Application.DTOs;
using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Enums;
using Hekucoreapp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly ICategoryLogger _categoryLogger;

    public AuthController(IAuthService authService, IConfiguration configuration, ICategoryLogger categoryLogger)
    {
        _authService = authService;
        _configuration = configuration;
        _categoryLogger = categoryLogger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(new RegisterRequest
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = dto.Password
            }, GetFrontendBaseUrl());
            return Ok(new AuthResponseDto
            {
                Token = result.Token,
                Email = result.Email,
                UserName = result.UserName,
                MustChangePassword = result.MustChangePassword,
                PreferredTheme = result.PreferredTheme,
                RequiresEmailConfirmation = result.RequiresEmailConfirmation
            });
        }
        catch (Exception ex)
        {
            _categoryLogger.LogError(LogCategory.Http, $"Unhandled exception in {nameof(AuthController)}", ex);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto.Email, dto.Password);
            return Ok(new AuthResponseDto
            {
                Token = result.Token,
                Email = result.Email,
                UserName = result.UserName,
                MustChangePassword = result.MustChangePassword,
                PreferredTheme = result.PreferredTheme
            });
        }
        catch (Exception ex)
        {
            _categoryLogger.LogError(LogCategory.Http, $"Unhandled exception in {nameof(AuthController)}", ex);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth(GoogleAuthDto dto)
    {
        try
        {
            var result = await _authService.LoginOrRegisterWithGoogleAsync(dto.IdToken);
            return Ok(new AuthResponseDto
            {
                Token = result.Token,
                Email = result.Email,
                UserName = result.UserName,
                MustChangePassword = result.MustChangePassword,
                PreferredTheme = result.PreferredTheme
            });
        }
        catch (Exception ex)
        {
            _categoryLogger.LogError(LogCategory.Http, $"Unhandled exception in {nameof(AuthController)}", ex);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto dto)
    {
        try
        {
            await _authService.ConfirmEmailAsync(dto.UserId, dto.Token);
            return Ok();
        }
        catch (Exception ex)
        {
            _categoryLogger.LogError(LogCategory.Http, $"Unhandled exception in {nameof(AuthController)}", ex);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationDto dto)
    {
        // Always 200 — never reveal whether the address is registered or already confirmed.
        await _authService.ResendConfirmationAsync(dto.Email, GetFrontendBaseUrl());
        return Ok();
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        try
        {
            await _authService.AssignRoleAsync(dto.Email, dto.Role);
            return Ok(new { message = $"{dto.Email} assigned to {dto.Role}" });
        }
        catch (Exception ex)
        {
            _categoryLogger.LogError(LogCategory.Http, $"Unhandled exception in {nameof(AuthController)}", ex);
            return BadRequest(ex.Message);
        }
    }

    // Where email confirmation links point. Configured App:FrontendBaseUrl wins; otherwise
    // fall back to this request's own origin, which is correct in production where the SPA is
    // served from the same host as the API.
    private string GetFrontendBaseUrl()
    {
        var configured = _configuration["App:FrontendBaseUrl"];
        return !string.IsNullOrWhiteSpace(configured)
            ? configured
            : $"{Request.Scheme}://{Request.Host}";
    }
}

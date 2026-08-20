using Hekucoreapp.Application.DTOs;
using Hekucoreapp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
            });
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
            return BadRequest(ex.Message);
        }
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
            return BadRequest(ex.Message);
        }
    }
}
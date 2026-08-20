using Hekucoreapp.Application.DTOs;
using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Interfaces;
using Hekucoreapp.Domain.Models;
using Microsoft.Extensions.Localization;
using Hekucoreapp.Application.Resources;

namespace Hekucoreapp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly IEmailService _emailService;
    private readonly IGoogleTokenValidator _googleTokenValidator;

    private readonly EmailTemplates _emailTemplates;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, IStringLocalizer<Messages> localizer, IEmailService emailService, EmailTemplates emailTemplates, IGoogleTokenValidator googleTokenValidator)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _localizer = localizer;
        _emailService = emailService;
        _emailTemplates = emailTemplates;
        _googleTokenValidator = googleTokenValidator;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var userId = await _userRepository.CreateUserAsync(new CreateUserRequest
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password
        });

        var (_, roles, mustChangePassword, preferredTheme) = await _userRepository.GetUserInfoAsync(userId);
        var token = await _tokenService.GenerateTokenAsync(new GenerateTokenRequest
        {
            UserId = userId,
            Email = request.Email,
            UserName = request.UserName,
            Roles = roles
        });

        var (subject, body) = _emailTemplates.Welcome(request.UserName);
        await _emailService.SendAsync(request.Email, subject, body);
        
        return new AuthResult
        {
            Token = token,
            UserId = userId,
            Email = request.Email,
            UserName = request.UserName,
            Roles = roles,
            MustChangePassword = mustChangePassword,
            PreferredTheme = preferredTheme
        };
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var userId = await _userRepository.ValidateUserAsync(email, password)
            ?? throw new Exception(_localizer["InvalidCredentials"]);

        var (userName, roles, mustChangePassword, preferredTheme) = await _userRepository.GetUserInfoAsync(userId);
        var token = await _tokenService.GenerateTokenAsync(new GenerateTokenRequest
        {
            UserId = userId,
            Email = email,
            UserName = userName,
            Roles = roles
        });

        return new AuthResult
        {
            Token = token,
            UserId = userId,
            Email = email,
            UserName = userName,
            Roles = roles,
            MustChangePassword = mustChangePassword,
            PreferredTheme = preferredTheme
        };
    }

    public async Task AssignRoleAsync(string email, string role)
    {
        await _userRepository.AssignRoleAsync(email, role);
    }

    public async Task<AuthResult> LoginOrRegisterWithGoogleAsync(string idToken)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(idToken);

        if (!googleUser.EmailVerified)
            throw new Exception(_localizer["GoogleEmailNotVerified"]);

        var (userId, isNewUser) = await _userRepository.FindOrCreateGoogleUserAsync(googleUser);

        var (userName, roles, mustChangePassword, preferredTheme) = await _userRepository.GetUserInfoAsync(userId);
        var token = await _tokenService.GenerateTokenAsync(new GenerateTokenRequest
        {
            UserId = userId,
            Email = googleUser.Email,
            UserName = userName,
            Roles = roles
        });

        if (isNewUser)
        {
            var (subject, body) = _emailTemplates.Welcome(userName);
            await _emailService.SendAsync(googleUser.Email, subject, body);
        }

        return new AuthResult
        {
            Token = token,
            UserId = userId,
            Email = googleUser.Email,
            UserName = userName,
            Roles = roles,
            MustChangePassword = mustChangePassword,
            PreferredTheme = preferredTheme
        };
    }
}
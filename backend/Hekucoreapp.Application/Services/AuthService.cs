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
    private readonly IAppSettingsRepository _appSettingsRepository;

    private readonly EmailTemplates _emailTemplates;

    public AuthService(IUserRepository userRepository, ITokenService tokenService, IStringLocalizer<Messages> localizer, IEmailService emailService, EmailTemplates emailTemplates, IGoogleTokenValidator googleTokenValidator, IAppSettingsRepository appSettingsRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _localizer = localizer;
        _emailService = emailService;
        _emailTemplates = emailTemplates;
        _googleTokenValidator = googleTokenValidator;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, string confirmationBaseUrl)
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        var requireConfirmation = settings.RequireEmailConfirmation;

        var userId = await _userRepository.CreateUserAsync(new CreateUserRequest
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password,
            EmailConfirmed = !requireConfirmation
        });

        // Confirmation required: send the link, issue no token, and tell the client to prompt
        // the user to check their inbox instead of treating this as a signed-in session.
        if (requireConfirmation)
        {
            var confirmToken = await _userRepository.GenerateEmailConfirmationTokenAsync(userId);
            var confirmationUrl = BuildConfirmationUrl(confirmationBaseUrl, userId, confirmToken);
            var (confirmSubject, confirmBody) = _emailTemplates.ConfirmEmail(request.UserName, confirmationUrl);
            await _emailService.SendAsync(request.Email, confirmSubject, confirmBody);

            return new AuthResult
            {
                UserId = userId,
                Email = request.Email,
                UserName = request.UserName,
                RequiresEmailConfirmation = true
            };
        }

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

        var settings = await _appSettingsRepository.GetSettingsAsync();
        if (settings.RequireEmailConfirmation && !await _userRepository.IsEmailConfirmedAsync(userId))
            throw new Exception(_localizer["EmailNotConfirmed"]);

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

    public async Task ConfirmEmailAsync(string userId, string token)
    {
        var confirmed = await _userRepository.ConfirmEmailAsync(userId, token);
        if (!confirmed)
            throw new Exception(_localizer["EmailConfirmationInvalid"]);
    }

    // Silent no-op when confirmation isn't required, the address isn't registered, or it's
    // already confirmed — the caller always returns 200 so this never reveals which.
    public async Task ResendConfirmationAsync(string email, string confirmationBaseUrl)
    {
        var settings = await _appSettingsRepository.GetSettingsAsync();
        if (!settings.RequireEmailConfirmation) return;

        var userId = await _userRepository.FindUserIdByEmailAsync(email);
        if (userId == null) return;
        if (await _userRepository.IsEmailConfirmedAsync(userId)) return;

        var profile = await _userRepository.GetProfileAsync(userId);
        var confirmToken = await _userRepository.GenerateEmailConfirmationTokenAsync(userId);
        var confirmationUrl = BuildConfirmationUrl(confirmationBaseUrl, userId, confirmToken);
        var (subject, body) = _emailTemplates.ConfirmEmail(profile.UserName, confirmationUrl);
        await _emailService.SendAsync(profile.Email, subject, body);
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

    private static string BuildConfirmationUrl(string baseUrl, string userId, string token) =>
        $"{baseUrl.TrimEnd('/')}/confirm-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
}

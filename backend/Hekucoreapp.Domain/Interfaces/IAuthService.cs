namespace Hekucoreapp.Domain.Interfaces;

using Hekucoreapp.Domain.Models;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, string confirmationBaseUrl);
    Task<AuthResult> LoginAsync(string email, string password);
    Task AssignRoleAsync(string email, string role);
    Task<AuthResult> LoginOrRegisterWithGoogleAsync(string idToken);
    Task ConfirmEmailAsync(string userId, string token);
    Task ResendConfirmationAsync(string email, string confirmationBaseUrl);
}
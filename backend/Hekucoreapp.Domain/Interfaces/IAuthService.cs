namespace Hekucoreapp.Domain.Interfaces;

using Hekucoreapp.Domain.Models;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LoginAsync(string email, string password);
    Task AssignRoleAsync(string email, string role);
    Task<AuthResult> LoginOrRegisterWithGoogleAsync(string idToken);
}
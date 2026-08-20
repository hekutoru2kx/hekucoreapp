namespace Hekucoreapp.Domain.Models;

public class AuthResult
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();

    public bool MustChangePassword { get; set; }

    public string PreferredTheme { get; set; } = "azure";
}
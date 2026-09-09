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

    // True when registration succeeded but the account is unconfirmed and email confirmation
    // is required — no token is issued and the client should prompt the user to check their
    // inbox instead of treating this as a logged-in session.
    public bool RequiresEmailConfirmation { get; set; }
}
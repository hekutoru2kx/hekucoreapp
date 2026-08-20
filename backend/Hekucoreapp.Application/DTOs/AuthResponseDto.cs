namespace Hekucoreapp.Application.DTOs;

public class AuthResponseDto
{
    public string UserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public bool MustChangePassword { get; set; }

    public string PreferredTheme { get; set; } = "azure";
}
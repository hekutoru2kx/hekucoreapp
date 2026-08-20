namespace Hekucoreapp.Domain.Models;

public class UserRoleHistoryResult
{
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }
}

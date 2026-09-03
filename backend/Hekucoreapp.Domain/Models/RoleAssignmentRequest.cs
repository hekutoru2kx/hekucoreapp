namespace Hekucoreapp.Domain.Models;

// One desired role for a user, with an optional validity window. Null StartsAt = effective
// immediately; null ExpiresAt = never expires on its own. The window is enforced at
// claim-resolution time (UserRoleRepository.GetActiveRoleNamesAsync), not just displayed.
public class RoleAssignmentRequest
{
    public string RoleName { get; set; } = string.Empty;
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

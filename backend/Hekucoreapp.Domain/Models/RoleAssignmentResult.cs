namespace Hekucoreapp.Domain.Models;

// A currently-open (non-revoked) role assignment as shown on the user's role-assignment page.
// Distinct from UserRoleHistoryResult, which is the flat grant/revoke trail including closed rows.
public class RoleAssignmentResult
{
    public string RoleName { get; set; } = string.Empty;
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Derived from the window vs. now, so the UI can badge the row without re-deriving the rule.
    public bool IsPending { get; set; }
    public bool IsExpired { get; set; }

    public DateTime CreatedAt { get; set; }
    // Resolved to a display name (userName, falling back to email, then the raw id), not the id.
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedByName { get; set; } = string.Empty;
}

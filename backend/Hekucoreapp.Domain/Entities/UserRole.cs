using Hekucoreapp.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hekucoreapp.Domain.Entities;

[Table("user_roles")]
public class UserRole : AuditableEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("role_id")]
    public string RoleId { get; set; } = string.Empty;

    // Null means the claims it grants are effective immediately. Distinct from CreatedAt —
    // a grant can be recorded today but scheduled to only take effect on a future date.
    // Enforced at claim-resolution time (GetActiveRoleNamesAsync), not just displayed.
    [Column("starts_at")]
    public DateTime? StartsAt { get; set; }

    // Null means the assignment never expires on its own.
    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    // Null means this assignment is currently open/held. Set when it's closed out early —
    // by reassignment (the user's role set was edited) — as opposed to simply running past
    // ExpiresAt on its own. Kept (never deleted) so the same role can be granted again later
    // while preserving full history.
    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }
}

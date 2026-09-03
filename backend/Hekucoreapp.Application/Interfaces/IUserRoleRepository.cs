using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface IUserRoleRepository
{
    Task<IList<string>> GetActiveRoleNamesAsync(string userId);

    // Additive single-role grant that leaves any other roles the user holds untouched.
    Task GrantRoleAsync(string userId, string roleName);

    // Replaces the user's whole active role set with the given assignments, diffing against what's
    // currently held: a kept role has its StartsAt/ExpiresAt updated in place (its CreatedAt/By is
    // preserved — an edit, not a new grant), a dropped role is soft-revoked, a new role opens a
    // fresh row.
    Task AssignRolesAsync(string userId, IList<RoleAssignmentRequest> assignments);

    // Currently-open assignments only, with grantor/editor ids resolved to display names and the
    // pending/expired flags computed. For editing on the role-assignment page.
    Task<IList<RoleAssignmentResult>> GetRoleAssignmentsAsync(string userId);

    // The flat grant/revoke trail, including closed rows. For the read-only history dialog.
    Task<IList<UserRoleHistoryResult>> GetHistoryAsync(string userId);
}

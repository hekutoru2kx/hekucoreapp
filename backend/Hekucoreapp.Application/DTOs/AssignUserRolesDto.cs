namespace Hekucoreapp.Application.DTOs;

public class AssignUserRolesDto
{
    // The full desired set of open assignments for the user. Roles absent from this list that the
    // user currently holds are revoked; roles present keep their row (dates updated in place); new
    // roles open a fresh row. See UserRoleRepository.AssignRolesAsync.
    public IList<RoleAssignmentInputDto> Roles { get; set; } = new List<RoleAssignmentInputDto>();
}

public class RoleAssignmentInputDto
{
    public string RoleName { get; set; } = string.Empty;
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

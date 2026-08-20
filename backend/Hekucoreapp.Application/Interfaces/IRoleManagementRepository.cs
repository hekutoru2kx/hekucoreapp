using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface IRoleManagementRepository
{
    Task<IList<RoleResult>> GetRolesAsync();
    Task CreateRoleAsync(string name);
    Task DeleteRoleAsync(string name);
    Task AssignClaimsAsync(string roleName, IList<PermissionClaimResult> claims);
    Task RestoreDefaultRolesAsync();
}

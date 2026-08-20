using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface IUserRoleRepository
{
    Task<IList<string>> GetActiveRoleNamesAsync(string userId);

    // Additive single-role grant that leaves any other roles the user holds untouched.
    Task GrantRoleAsync(string userId, string roleName);

    // Replaces the user's whole active role set, diffing against what's currently held.
    Task AssignRolesAsync(string userId, IList<string> roleNames);

    Task<IList<UserRoleHistoryResult>> GetHistoryAsync(string userId);
}

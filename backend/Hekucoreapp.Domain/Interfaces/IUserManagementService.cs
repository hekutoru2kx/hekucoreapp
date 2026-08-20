using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Domain.Interfaces;

public interface IUserManagementService
{
    Task<PagedResult<UserListResult>> GetUsersAsync(UserListQuery query);
    
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest request);
    Task AssignRolesAsync(string userId, IList<string> roles);
    Task<IList<UserRoleHistoryResult>> GetRoleHistoryAsync(string userId);
    Task DeactivateUserAsync(string userId);
    Task ActivateUserAsync(string userId);
    Task ResetPasswordAsync(string userId, string newPassword);
    Task DeleteUserAsync(string userId);

    Task LinkPersonAsync(string userId, int personId);
    Task UnlinkPersonAsync(string userId);
}
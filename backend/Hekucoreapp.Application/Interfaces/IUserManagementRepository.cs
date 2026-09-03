using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Application.Interfaces;

public interface IUserManagementRepository
{
    Task<PagedResult<UserListResult>> GetUsersAsync(UserListQuery query);
    Task<UserListResult?> GetUserByIdAsync(string userId);
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest request);
    Task AssignRolesAsync(string userId, IList<RoleAssignmentRequest> assignments);
    Task<IList<RoleAssignmentResult>> GetRoleAssignmentsAsync(string userId);
    Task<IList<UserRoleHistoryResult>> GetRoleHistoryAsync(string userId);
    Task DeactivateUserAsync(string userId);
    Task ActivateUserAsync(string userId);
    Task ResetPasswordAsync(string userId, string newPassword);
    Task DeleteUserAsync(string userId);

    Task LinkPersonAsync(string userId, int personId);
    Task UnlinkPersonAsync(string userId);
}
using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Models;
using Hekucoreapp.Domain.Interfaces;

namespace Hekucoreapp.Application.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserManagementRepository _repository;
    private readonly IEmailService _emailService;
    private readonly EmailTemplates _emailTemplates;

    public UserManagementService(IUserManagementRepository repository, IEmailService emailService, EmailTemplates emailTemplates)
    {
        _repository = repository;
        _emailService = emailService;
        _emailTemplates = emailTemplates;
    }

    public async Task<PagedResult<UserListResult>> GetUsersAsync(UserListQuery query) =>
        await _repository.GetUsersAsync(query);

    public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request) =>
    await _repository.CreateUserAsync(request);

    public async Task AssignRolesAsync(string userId, IList<string> roles) =>
        await _repository.AssignRolesAsync(userId, roles);

    public async Task<IList<UserRoleHistoryResult>> GetRoleHistoryAsync(string userId) =>
        await _repository.GetRoleHistoryAsync(userId);

    public async Task DeactivateUserAsync(string userId) =>
        await _repository.DeactivateUserAsync(userId);

    public async Task ActivateUserAsync(string userId) =>
        await _repository.ActivateUserAsync(userId);

    public async Task ResetPasswordAsync(string userId, string newPassword)
    {
        await _repository.ResetPasswordAsync(userId, newPassword);

        var user = await _repository.GetUserByIdAsync(userId);
        if (user != null)
        {
            var (subject, body) = _emailTemplates.PasswordReset(
                user.UserName,
                newPassword);
            await _emailService.SendAsync(user.Email, subject, body);
        }
    }

    public async Task<UserListResult?> GetUserByIdAsync(string userId) =>
        await _repository.GetUserByIdAsync(userId);
    public async Task DeleteUserAsync(string userId) =>
        await _repository.DeleteUserAsync(userId);

    public async Task LinkPersonAsync(string userId, int personId) =>
        await _repository.LinkPersonAsync(userId, personId);

    public async Task UnlinkPersonAsync(string userId) =>
        await _repository.UnlinkPersonAsync(userId);
}
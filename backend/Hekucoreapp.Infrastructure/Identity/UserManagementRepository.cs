using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Hekucoreapp.Application.Resources;
using Microsoft.EntityFrameworkCore;
using Hekucoreapp.Infrastructure.Data;

namespace Hekucoreapp.Infrastructure.Identity;

public class UserManagementRepository : IUserManagementRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly HekucoreappDbContext _context;
    private readonly IUserRoleRepository _userRoleRepository;

    public UserManagementRepository(UserManager<ApplicationUser> userManager, IStringLocalizer<Messages> localizer, HekucoreappDbContext context, IUserRoleRepository userRoleRepository)
    {
        _userManager = userManager;
        _localizer = localizer;
        _context = context;
        _userRoleRepository = userRoleRepository;
    }

    public async Task<UserListResult?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userRoleRepository.GetActiveRoleNamesAsync(user.Id);

        return new UserListResult
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<PagedResult<UserListResult>> GetUsersAsync(UserListQuery query)
    {
        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search)));
        }

        if (query.StatusFilter.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.IsActive == query.StatusFilter.Value);
        }

        usersQuery = query.SortBy?.ToLower() switch
        {
            "email" => query.SortDirection == "desc" ? usersQuery.OrderByDescending(u => u.Email) : usersQuery.OrderBy(u => u.Email),
            "createdat" => query.SortDirection == "desc" ? usersQuery.OrderByDescending(u => u.CreatedAt) : usersQuery.OrderBy(u => u.CreatedAt),
            _ => query.SortDirection == "desc" ? usersQuery.OrderByDescending(u => u.UserName) : usersQuery.OrderBy(u => u.UserName)
        };

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var result = new List<UserListResult>();

        foreach (var user in users)
        {
            var roles = await _userRoleRepository.GetActiveRoleNamesAsync(user.Id);

            if (!string.IsNullOrEmpty(query.RoleFilter) && !roles.Contains(query.RoleFilter))
                continue;

            result.Add(new UserListResult
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                CreatedAt = user.CreatedAt,
                PersonId = user.PersonId
            });
        }

        return new PagedResult<UserListResult>
        {
            Items = result,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
    public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.UserName,
            MustChangePassword = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!string.IsNullOrEmpty(request.Role))
            await _userRoleRepository.GrantRoleAsync(user.Id, request.Role);

        return new CreateUserResult
        {
            UserId = user.Id,
            Email = request.Email,
            UserName = request.UserName,
            TemporaryPassword = request.Password
        };
    }

    public async Task AssignRolesAsync(string userId, IList<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        await _userRoleRepository.AssignRolesAsync(userId, roles);
    }

    public async Task<IList<UserRoleHistoryResult>> GetRoleHistoryAsync(string userId) =>
        await _userRoleRepository.GetHistoryAsync(userId);

    public async Task DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
    }

    public async Task ActivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        user.IsActive = true;
        await _userManager.UpdateAsync(user);
    }

    public async Task ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        var normalizedEmail = user.NormalizedEmail;

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (!string.IsNullOrEmpty(normalizedEmail))
        {
            _context.DeletedAccounts.Add(new DeletedAccount { NormalizedEmail = normalizedEmail });
            await _context.SaveChangesAsync();
        }
    }

    public async Task LinkPersonAsync(string userId, int personId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        // Check person exists
        var person = await _context.Persons.FindAsync(personId)
            ?? throw new Exception(_localizer["PersonNotFound"]);

        // Check person not already linked to another user
        var alreadyLinked = await _userManager.Users
            .AnyAsync(u => u.PersonId == personId && u.Id != userId);

        if (alreadyLinked)
            throw new Exception(_localizer["PersonAlreadyLinkedToAnotherAccount"]);

        user.PersonId = personId;
        await _userManager.UpdateAsync(user);
    }

    public async Task UnlinkPersonAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new Exception(_localizer["UserNotFound"]);

        user.PersonId = null;
        await _userManager.UpdateAsync(user);
    }
}
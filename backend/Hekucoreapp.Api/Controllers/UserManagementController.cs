using Hekucoreapp.Application.DTOs;
using Hekucoreapp.Domain.Enums.Permissions;
using Hekucoreapp.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hekucoreapp.Domain.Models;

namespace Hekucoreapp.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Read))]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? sortBy = "UserName",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] string? search = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] bool? statusFilter = null)
    {
        var query = new UserListQuery
        {
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Search = search,
            RoleFilter = roleFilter,
            StatusFilter = statusFilter
        };

        var result = await _userManagementService.GetUsersAsync(query);

        return Ok(new
        {
            items = result.Items.Select(u => new UserListDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = u.Roles,
                IsActive = u.IsActive,
                MustChangePassword = u.MustChangePassword,
                CreatedAt = u.CreatedAt,
                PersonId = u.PersonId
            }),
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize
        });
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Create))]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        try
        {
            var password = string.IsNullOrEmpty(dto.Password)
                ? GeneratePassword()
                : dto.Password;

            var result = await _userManagementService.CreateUserAsync(new CreateUserRequest
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = password,
                Role = dto.Role
            });

            return Ok(new CreateUserResponseDto
            {
                UserId = result.UserId,
                Email = result.Email,
                UserName = result.UserName,
                TemporaryPassword = result.TemporaryPassword
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/roles")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Update))]
    public async Task<IActionResult> AssignRoles(string id, AssignRolesDto dto)
    {
        try
        {
            await _userManagementService.AssignRolesAsync(id, dto.Roles);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}/roles/history")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Read))]
    public async Task<IActionResult> GetRoleHistory(string id)
    {
        var history = await _userManagementService.GetRoleHistoryAsync(id);
        return Ok(history.Select(h => new
        {
            roleName = h.RoleName,
            createdAt = h.CreatedAt,
            createdBy = h.CreatedBy,
            revokedAt = h.RevokedAt,
            revokedBy = h.RevokedBy
        }));
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Update))]
    public async Task<IActionResult> Deactivate(string id)
    {
        try
        {
            await _userManagementService.DeactivateUserAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/activate")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Update))]
    public async Task<IActionResult> Activate(string id)
    {
        try
        {
            await _userManagementService.ActivateUserAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/reset-password")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Update))]
    public async Task<IActionResult> ResetPassword(string id, ResetPasswordDto dto)
    {
        try
        {
            var password = string.IsNullOrEmpty(dto.NewPassword)
                ? GeneratePassword()
                : dto.NewPassword;

            await _userManagementService.ResetPasswordAsync(id, password);
            return Ok(new { temporaryPassword = password });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Delete))]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            await _userManagementService.DeleteUserAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static string GeneratePassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        var password = new string(Enumerable.Range(0, 12)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
        return password + "A1!";
    }

    [HttpPut("{userId}/link-person/{personId}")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Update))]
    public async Task<IActionResult> LinkPerson(string userId, int personId)
    {
        try
        {
            await _userManagementService.LinkPersonAsync(userId, personId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{userId}/unlink-person")]
    [Authorize(Policy = nameof(UserManagementPermission) + "." + nameof(UserManagementPermission.Update))]
    public async Task<IActionResult> UnlinkPerson(string userId)
    {
        try
        {
            await _userManagementService.UnlinkPersonAsync(userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Application.Resources;
using Hekucoreapp.Domain.Entities;
using Hekucoreapp.Domain.Models;
using Hekucoreapp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Hekucoreapp.Infrastructure.Identity;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly HekucoreappDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IStringLocalizer<Messages> _localizer;

    public UserRoleRepository(HekucoreappDbContext context, RoleManager<IdentityRole> roleManager, IStringLocalizer<Messages> localizer)
    {
        _context = context;
        _roleManager = roleManager;
        _localizer = localizer;
    }

    public async Task<IList<string>> GetActiveRoleNamesAsync(string userId)
    {
        var now = DateTime.UtcNow;

        var roleIds = await _context.UserRoleAssignments
            .Where(ur => ur.UserId == userId
                && ur.RevokedAt == null
                && (ur.StartsAt == null || ur.StartsAt <= now)
                && (ur.ExpiresAt == null || ur.ExpiresAt > now))
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (roleIds.Count == 0) return [];

        return await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync();
    }

    public async Task GrantRoleAsync(string userId, string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName)
            ?? throw new Exception(_localizer["RoleNotFound"]);

        var alreadyActive = await _context.UserRoleAssignments.AnyAsync(ur =>
            ur.UserId == userId && ur.RoleId == role.Id && ur.RevokedAt == null);
        if (alreadyActive) return;

        _context.UserRoleAssignments.Add(new UserRole { UserId = userId, RoleId = role.Id });
        await _context.SaveChangesAsync();
    }

    // Diffs the desired role set against currently-active rows: a role kept in both is left
    // untouched (preserving its original CreatedAt/CreatedBy), a dropped role is soft-revoked
    // (row kept for history), a new role opens a fresh row.
    public async Task AssignRolesAsync(string userId, IList<string> roleNames)
    {
        var roleIdByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in roleNames.Distinct())
        {
            var role = await _roleManager.FindByNameAsync(name)
                ?? throw new Exception(_localizer["RoleNotFound"]);
            roleIdByName[name] = role.Id;
        }

        var activeRows = await _context.UserRoleAssignments
            .Where(ur => ur.UserId == userId && ur.RevokedAt == null)
            .ToListAsync();

        var desiredRoleIds = roleIdByName.Values.ToHashSet();
        var activeRoleIds = activeRows.Select(r => r.RoleId).ToHashSet();

        var now = DateTime.UtcNow;
        foreach (var row in activeRows.Where(r => !desiredRoleIds.Contains(r.RoleId)))
            row.RevokedAt = now;

        foreach (var roleId in desiredRoleIds.Except(activeRoleIds))
            _context.UserRoleAssignments.Add(new UserRole { UserId = userId, RoleId = roleId });

        await _context.SaveChangesAsync();
    }

    public async Task<IList<UserRoleHistoryResult>> GetHistoryAsync(string userId)
    {
        var rows = await _context.UserRoleAssignments
            .Where(ur => ur.UserId == userId)
            .OrderByDescending(ur => ur.CreatedAt)
            .ToListAsync();

        var roleIds = rows.Select(r => r.RoleId).Distinct().ToList();
        var roleNames = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name ?? string.Empty);

        return rows.Select(ur => new UserRoleHistoryResult
        {
            RoleName = roleNames.GetValueOrDefault(ur.RoleId, ur.RoleId),
            CreatedAt = ur.CreatedAt,
            CreatedBy = ur.CreatedBy,
            RevokedAt = ur.RevokedAt,
            RevokedBy = ur.RevokedAt != null ? ur.UpdatedBy : null
        }).ToList();
    }
}

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

    // Diffs the desired assignments against currently-open rows: a role kept in both has its
    // StartsAt/ExpiresAt updated in place (its original CreatedAt/CreatedBy survive — this is an
    // edit, not a re-grant), a dropped role is soft-revoked (row kept for history), a new role
    // opens a fresh row.
    public async Task AssignRolesAsync(string userId, IList<RoleAssignmentRequest> assignments)
    {
        var roleIdByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var assignment in assignments)
        {
            if (roleIdByName.ContainsKey(assignment.RoleName)) continue;
            var role = await _roleManager.FindByNameAsync(assignment.RoleName)
                ?? throw new Exception(_localizer["RoleNotFound"]);
            roleIdByName[assignment.RoleName] = role.Id;
        }

        var openRows = await _context.UserRoleAssignments
            .Where(ur => ur.UserId == userId && ur.RevokedAt == null)
            .ToListAsync();

        var desiredRoleIds = roleIdByName.Values.ToHashSet();

        var now = DateTime.UtcNow;
        foreach (var row in openRows.Where(r => !desiredRoleIds.Contains(r.RoleId)))
            row.RevokedAt = now;

        foreach (var assignment in assignments)
        {
            var roleId = roleIdByName[assignment.RoleName];
            var existing = openRows.FirstOrDefault(r => r.RoleId == roleId);
            if (existing != null)
            {
                existing.StartsAt = assignment.StartsAt;
                existing.ExpiresAt = assignment.ExpiresAt;
            }
            else
            {
                _context.UserRoleAssignments.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    StartsAt = assignment.StartsAt,
                    ExpiresAt = assignment.ExpiresAt
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IList<RoleAssignmentResult>> GetRoleAssignmentsAsync(string userId)
    {
        var rows = await _context.UserRoleAssignments
            .Where(ur => ur.UserId == userId && ur.RevokedAt == null)
            .ToListAsync();

        if (rows.Count == 0) return [];

        var roleIds = rows.Select(r => r.RoleId).Distinct().ToList();
        var roleNames = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.Name ?? string.Empty);

        // CreatedBy/UpdatedBy hold the acting user's id (see HekucoreappDbContext.SaveChangesAsync);
        // resolve to something readable, falling back through email to the raw id.
        var actorIds = rows.Select(r => r.CreatedBy)
            .Concat(rows.Select(r => r.UpdatedBy))
            .Distinct()
            .ToList();
        var actorNames = await _context.Users
            .Where(u => actorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? u.Id);

        var now = DateTime.UtcNow;
        return rows.Select(ur => new RoleAssignmentResult
        {
            RoleName = roleNames.GetValueOrDefault(ur.RoleId, ur.RoleId),
            StartsAt = ur.StartsAt,
            ExpiresAt = ur.ExpiresAt,
            IsPending = ur.StartsAt != null && ur.StartsAt > now,
            IsExpired = ur.ExpiresAt != null && ur.ExpiresAt <= now,
            CreatedAt = ur.CreatedAt,
            CreatedByName = actorNames.GetValueOrDefault(ur.CreatedBy, ur.CreatedBy),
            UpdatedAt = ur.UpdatedAt,
            UpdatedByName = actorNames.GetValueOrDefault(ur.UpdatedBy, ur.UpdatedBy)
        }).ToList();
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

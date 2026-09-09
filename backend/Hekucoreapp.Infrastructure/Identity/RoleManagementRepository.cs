using Hekucoreapp.Application.Interfaces;
using Hekucoreapp.Domain.Catalogs;
using Hekucoreapp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Hekucoreapp.Application.Resources;
using System.Security.Claims;

namespace Hekucoreapp.Infrastructure.Identity;

public class RoleManagementRepository : IRoleManagementRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IStringLocalizer<Messages> _localizer;

    public RoleManagementRepository(RoleManager<IdentityRole> roleManager, IStringLocalizer<Messages> localizer)
    {
        _roleManager = roleManager;
        _localizer = localizer;
    }

    public async Task<IList<RoleResult>> GetRolesAsync()
    {
        var result = new List<RoleResult>();

        foreach (var role in _roleManager.Roles.ToList())
        {
            var claims = await _roleManager.GetClaimsAsync(role);

            result.Add(new RoleResult
            {
                Name = role.Name ?? string.Empty,
                Claims = claims
                    .Select(c => new PermissionClaimResult { Module = c.Type, Action = c.Value })
                    .ToList()
            });
        }

        return result;
    }

    public async Task CreateRoleAsync(string name)
    {
        if (await _roleManager.RoleExistsAsync(name))
            throw new Exception(_localizer["RoleAlreadyExists"]);

        var result = await _roleManager.CreateAsync(new IdentityRole(name));
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task DeleteRoleAsync(string name)
    {
        if (string.Equals(name, "Admin", StringComparison.OrdinalIgnoreCase))
            throw new Exception(_localizer["CannotDeleteAdminRole"]);

        var role = await _roleManager.FindByNameAsync(name)
            ?? throw new Exception(_localizer["RoleNotFound"]);

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task AssignClaimsAsync(string roleName, IList<PermissionClaimResult> claims)
    {
        var role = await _roleManager.FindByNameAsync(roleName)
            ?? throw new Exception(_localizer["RoleNotFound"]);

        var currentClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var claim in currentClaims)
            await _roleManager.RemoveClaimAsync(role, claim);

        foreach (var claim in claims)
            await _roleManager.AddClaimAsync(role, new Claim(claim.Module, claim.Action));
    }

    // Creates any catalog role that doesn't exist yet, seeding it with its catalog claims, and
    // leaves roles that already exist completely untouched. Called at startup.
    //
    // Deliberately NOT RestoreDefaultRolesAsync: that one reconciles fully (removes stray claims),
    // which is correct for an admin explicitly pressing "restore defaults" but would silently undo
    // their customisations on every app restart. Seed-on-first-create is the boot-safe half.
    //
    // Without this, a catalog role only ever comes into existence when an admin hits the
    // restore-defaults endpoint by hand — so on a fresh database GrantRoleAsync throws
    // RoleNotFound for anything that grants a default role.
    public async Task EnsureDefaultRolesExistAsync()
    {
        foreach (var group in DefaultRoleCatalog.Roles.GroupBy(r => r.RoleName))
        {
            if (await _roleManager.FindByNameAsync(group.Key) != null) continue;

            var role = new IdentityRole(group.Key);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            var desired = group
                .SelectMany(g => g.Actions.Select(action => (Module: g.Module, Action: action)))
                .ToHashSet();

            foreach (var (module, action) in desired)
                await _roleManager.AddClaimAsync(role, new Claim(module, action));
        }
    }

    public async Task RestoreDefaultRolesAsync()
    {
        // Reconciles fully (adds missing, removes stray) rather than only adding — otherwise
        // a claim that's since been dropped from the catalog would survive forever on a role
        // created before the change.
        foreach (var group in DefaultRoleCatalog.Roles.GroupBy(r => r.RoleName))
        {
            var role = await _roleManager.FindByNameAsync(group.Key);
            if (role == null)
            {
                role = new IdentityRole(group.Key);
                await _roleManager.CreateAsync(role);
            }

            var desired = group
                .SelectMany(g => g.Actions.Select(action => (Module: g.Module, Action: action)))
                .ToHashSet();

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            foreach (var claim in existingClaims.Where(c => !desired.Contains((c.Type, c.Value))))
                await _roleManager.RemoveClaimAsync(role, claim);

            foreach (var (module, action) in desired)
            {
                if (!existingClaims.Any(c => c.Type == module && c.Value == action))
                    await _roleManager.AddClaimAsync(role, new Claim(module, action));
            }
        }
    }
}

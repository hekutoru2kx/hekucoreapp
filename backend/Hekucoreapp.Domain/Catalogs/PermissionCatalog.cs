using Hekucoreapp.Domain.Enums.Permissions;

namespace Hekucoreapp.Domain.Catalogs;

public static class PermissionCatalog
{
    public static readonly (string Module, Type EnumType)[] Modules =
    [
        (nameof(RolesPermission), typeof(RolesPermission)),
        (nameof(UserManagementPermission), typeof(UserManagementPermission)),
        (nameof(PersonsPermission), typeof(PersonsPermission)),
    ];
}

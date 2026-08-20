using Hekucoreapp.Domain.Enums.Permissions;

namespace Hekucoreapp.Domain.Catalogs;

public static class DefaultRoleCatalog
{
    public static readonly (string RoleName, string Module, string[] Actions)[] Roles =
    [
        ("UserManagementRole", nameof(UserManagementPermission), AllActions<UserManagementPermission>()),
        ("UserManagementRole", nameof(PersonsPermission), AllActions<PersonsPermission>()),
        ("PersonManagementRole", nameof(PersonsPermission), AllActions<PersonsPermission>()),
    ];

    private static string[] AllActions<TEnum>() where TEnum : struct, Enum => Enum.GetNames<TEnum>();
}

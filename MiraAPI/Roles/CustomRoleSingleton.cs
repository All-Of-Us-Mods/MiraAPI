using System.Linq;

namespace MiraAPI.Roles;

/// <summary>
/// A utility class to get the instance of a custom role.
/// </summary>
/// <typeparam name="T">The role you are trying to access.</typeparam>
public static class CustomRoleSingleton<T> where T : ICustomRole
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the role.
    /// </summary>
#pragma warning disable CA1000
    public static ICustomRole Instance => _instance ??= CustomRoleManager.CustomRoles.Values.OfType<T>().Single();
#pragma warning restore CA1000
}

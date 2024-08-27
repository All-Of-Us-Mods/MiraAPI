using System;
using System.Linq;

namespace MiraAPI.Roles;

public class RoleId
{
    public static ushort Get<T>() where T : RoleBehaviour
    {
        if (!CustomRoleManager.RoleIds.TryGetValue(typeof(T), out var roleId))
        {
            throw new InvalidOperationException($"Role {typeof(T)} is not registered");
        }

        return roleId;
    }
}
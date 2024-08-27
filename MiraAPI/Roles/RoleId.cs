using System;

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
    
    public static ushort Get(Type type)
    {
        if (!CustomRoleManager.RoleIds.TryGetValue(type, out var roleId))
        {
            throw new InvalidOperationException($"Role {type} is not registered");
        }

        return roleId;
    }
}
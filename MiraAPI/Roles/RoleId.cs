using System;
using System.Linq;

namespace MiraAPI.Roles;

public class RoleId
{
    public static ushort Get<T>() where T : RoleBehaviour
    {
        if (!CustomRoleManager.CustomRoles.Values.OfType<T>().Any())
        {
            throw new InvalidOperationException($"Role {typeof(T)} is not registered");
        }
        
        return (ushort)CustomRoleManager.CustomRoles.Values.OfType<T>().Single().Role;
    }
}
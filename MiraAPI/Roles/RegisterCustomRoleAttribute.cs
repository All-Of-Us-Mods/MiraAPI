using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.Roles;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomRoleAttribute : Attribute
{
    private static readonly HashSet<Assembly> RegisteredAssemblies = [];

    public static void Register(Assembly assembly)
    {
        if (!RegisteredAssemblies.Add(assembly)) return;

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRoleAttribute>();
            if (attribute != null)
            {
                CustomRoleManager.RegisterRole(type);
            }
        }
    }
}
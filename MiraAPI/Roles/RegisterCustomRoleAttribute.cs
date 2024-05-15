using BepInEx.Unity.IL2CPP;
using MiraAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.Roles;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomRoleAttribute : Attribute
{
    private static readonly HashSet<Assembly> RegisteredAssemblies = [];
    public ushort RoleId { get; }
    public string ModId { get; }

    public RegisterCustomRoleAttribute(string modId)
    {
        ModId = modId;
        RoleId = (ushort)(20 + CustomRoleManager.CustomRoles.Count);
    }
    public RegisterCustomRoleAttribute(string modId, ushort roleId)
    {
        ModId = modId;
        RoleId = roleId;
    }

    public static void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) => Register(assembly);
    }

    public static void Register(Assembly assembly)
    {
        if (!RegisteredAssemblies.Add(assembly)) return;

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRoleAttribute>();
            if (attribute != null)
            {
                Helpers.RegisterType(type);
                CustomRoleManager.RegisterRole(type, attribute.RoleId, attribute.ModId);
            }
        }
    }
}
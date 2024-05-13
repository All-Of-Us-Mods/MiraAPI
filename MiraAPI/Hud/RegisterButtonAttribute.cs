using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.Hud;

public class RegisterButtonAttribute : Attribute
{
    private static readonly HashSet<Assembly> RegisteredAssemblies = [];

    public static void Register(Assembly assembly)
    {
        if (!RegisteredAssemblies.Add(assembly)) return;

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterButtonAttribute>();
            if (attribute != null)
            {
                CustomButtonManager.RegisterButton(type);
            }
        }
    }
}
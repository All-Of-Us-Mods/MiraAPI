using System;
using System.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime.Injection;
using MiraAPI.PluginLoading;

namespace MiraAPI.GameModes;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterGameModeAttribute : Attribute
{
    private static readonly HashSet<Assembly> RegisteredAssemblies = [];

    internal static void Register(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        if (!RegisteredAssemblies.Add(assembly))
        {
            return;
        }

        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterGameModeAttribute>();
            if (attribute != null)
            {
                CustomGameModeManager.RegisterGameMode(type);
            }
        }

        var dict = new Dictionary<string, object>();

        foreach (var (id, gameMode) in CustomGameModeManager.GameModes)
        {
            dict[pluginInfo.PluginInfo.Metadata.Name+": "+gameMode.Name] = id;
        }
        
        
        EnumInjector.InjectEnumValues<AmongUs.GameOptions.GameModes>(dict);
    }
}
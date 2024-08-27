using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities.Colors;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiraAPI.Utilities;
using MiraAPI.Hud;
using Reactor.Networking;

namespace MiraAPI.PluginLoading;

internal class MiraPluginManager
{
    public readonly Dictionary<Assembly, MiraPluginInfo> RegisteredPlugins = [];

    private static MiraPluginManager _instance;

    public static MiraPluginManager Instance
    {
        get => _instance ??= new MiraPluginManager();
        private set => _instance = value;
    }

    internal void Initialize()
    {
        Instance = this;
        IL2CPPChainloader.Instance.PluginLoad += (pluginInfo, assembly, plugin) =>
        {
            if (!plugin.GetType().GetInterfaces().Contains(typeof(IMiraPlugin)))
            {
                return;
            }

            var info = new MiraPluginInfo(plugin as IMiraPlugin, pluginInfo);

            RegisterAllOptions(assembly, info);
            
            RegisterRoleAttribute(assembly, info);
            RegisterButtonAttribute(assembly);

            RegisterColorClasses(assembly);

            RegisteredPlugins.Add(assembly, info);

            Logger<MiraApiPlugin>.Info($"Registering mod {pluginInfo.Metadata.GUID} with Mira API.");
        };
    }

    public MiraPluginInfo GetPluginByGuid(string guid)
    {
        return RegisteredPlugins.Values.First(plugin => plugin.PluginId == guid);
    }

    private static void RegisterAllOptions(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        var filteredTypes = assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(AbstractOptionGroup)));

        foreach (var type in filteredTypes)
        {
            if (!ModdedOptionsManager.RegisterGroup(type, pluginInfo))
            {
                continue;
            }
            
            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType.IsAssignableTo(typeof(IModdedOption)))
                {
                    ModdedOptionsManager.RegisterPropertyOption(type, property, pluginInfo);
                    continue;
                }
                
                var attribute = property.GetCustomAttribute<ModdedOptionAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                ModdedOptionsManager.RegisterAttributeOption(type, attribute, property, pluginInfo);
            }
        }
    }

    private static void RegisterRoleAttribute(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRoleAttribute>();
            if (attribute == null)
            {
                continue;
            }
            
            if (!ModList.GetById(pluginInfo.PluginId).IsRequiredOnAllClients)
            {
                Logger<MiraApiPlugin>.Error("Custom roles are only supported on all clients.");
                return;
            }

            ClassInjector.RegisterTypeInIl2Cpp(type);

            var role = CustomRoleManager.RegisterRole(type, pluginInfo);

            try
            {
                pluginInfo.CustomRoles.Add((ushort)role.Role, role);
            }
            catch (Exception e)
            {
                Logger<MiraApiPlugin>.Error("Failed to register role: " + type.Name);

                foreach (var (k, v) in pluginInfo.CustomRoles)
                {
                    Logger<MiraApiPlugin>.Error($"{k}: {v}");
                }
                
                Logger<MiraApiPlugin>.Error(e);
            }
        }
    }

    private static void RegisterColorClasses(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<RegisterCustomColorsAttribute>() == null)
            {
                continue;
            }

            if (!type.IsStatic())
            {
                Logger<MiraApiPlugin>.Error($"Color class {type.Name} must be static.");
                continue;
            }

            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType != typeof(CustomColor))
                {
                    continue;
                }
                
                var color = (CustomColor)property.GetValue(null);
                if (color == null)
                {
                    Logger<MiraApiPlugin>.Error($"Color property {property.Name} in {type.Name} is null.");
                    continue;
                }
                
                PaletteManager.CustomColors.Add(color);
            }
        }
    }

    private static void RegisterButtonAttribute(Assembly assembly)
    {
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
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Roles;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiraAPI.PluginLoading;

public class MiraPluginManager
{
    public readonly Dictionary<Assembly, MiraPluginInfo> RegisteredPlugins = [];
    
    private static MiraPluginManager _instance;
    
    public static MiraPluginManager Instance { 
        get => _instance ??= new MiraPluginManager();
        private set => _instance = value; 
    }

    public void Initialize()
    {
        Instance = this;
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) =>
        {
            if (!plugin.GetType().GetInterfaces().Contains(typeof(IMiraPlugin))) return;

            var id = MetadataHelper.GetMetadata(plugin.GetType()).GUID;
            var info = new MiraPluginInfo(id, plugin as IMiraPlugin, IL2CPPChainloader.Instance.Plugins[id]);

            RegisterRoleAttribute(assembly, info);
            RegisterOptionsGroups(assembly, info);
            RegisterOptionsAttributes(assembly, info);

            RegisteredPlugins.Add(assembly, info);

            Logger<MiraAPIPlugin>.Info($"Registering mod {id} with Mira API.");
        };
    }

    public MiraPluginInfo GetPluginByGuid(string guid)
    {
        return RegisteredPlugins.Values.First(plugin => plugin.PluginId == guid);
    }


    private static void RegisterOptionsAttributes(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (PropertyInfo property in type.GetProperties())
            {
                foreach (var attribute in property.GetCustomAttributes())
                {
                    if (attribute.GetType().BaseType == typeof(ModdedOptionAttribute))
                    {
                        pluginInfo.Options.Add(ModdedOptionsManager.RegisterOption(type, (ModdedOptionAttribute)attribute, property));
                    }
                }
            }
        }
    }

    private static void RegisterOptionsGroups(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(IModdedOptionGroup).IsAssignableFrom(type))
            {
                IModdedOptionGroup group = (IModdedOptionGroup)Activator.CreateInstance(type);
                ModdedOptionGroup newGroup = new ModdedOptionGroup
                {
                    AdvancedRole = group.AdvancedRole,
                    GroupColor = group.GroupColor,
                    GroupName = group.GroupName,
                    GroupVisible = group.GroupVisible
                };

                ModdedOptionsManager.Groups.Add(newGroup);
                ModdedOptionsManager.OriginalTypes.Add(type, newGroup);
                pluginInfo.OptionGroups.Add(group);
            }
        }
    }

    private static void RegisterRoleAttribute(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRoleAttribute>();
            if (attribute == null) continue;

            ClassInjector.RegisterTypeInIl2Cpp(type);

            var role = CustomRoleManager.RegisterRole(type, attribute.RoleId);

            pluginInfo.CustomRoles.Add((ushort)role.Role, role);
        }
    }
}
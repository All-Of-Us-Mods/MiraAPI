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
using MiraAPI.Hud;

namespace MiraAPI.PluginLoading;

internal class MiraPluginManager
{
    public readonly Dictionary<Assembly, MiraPluginInfo> RegisteredPlugins = [];
    
    private static MiraPluginManager _instance;
    
    public static MiraPluginManager Instance { 
        get => _instance ??= new MiraPluginManager();
        private set => _instance = value; 
    }

    internal void Initialize()
    {
        Instance = this;
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) =>
        {
            if (!plugin.GetType().GetInterfaces().Contains(typeof(IMiraPlugin)))
            {
                return;
            }

            var id = MetadataHelper.GetMetadata(plugin.GetType()).GUID;
            var info = new MiraPluginInfo(id, plugin as IMiraPlugin, IL2CPPChainloader.Instance.Plugins[id]);

            RegisterAllOptions(assembly, info);
            
            RegisterRoleAttribute(assembly, info);
            RegisterButtonAttribute(assembly);

            RegisteredPlugins.Add(assembly, info);

            Logger<MiraApiPlugin>.Info($"Registering mod {id} with Mira API.");
        };
    }

    public MiraPluginInfo GetPluginByGuid(string guid)
    {
        return RegisteredPlugins.Values.First(plugin => plugin.PluginId == guid);
    }

    private static void RegisterAllOptions(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        var filteredTypes = assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IModdedOptionGroup)));
        
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

            ClassInjector.RegisterTypeInIl2Cpp(type);

            var role = CustomRoleManager.RegisterRole(type, attribute.RoleId, pluginInfo);

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
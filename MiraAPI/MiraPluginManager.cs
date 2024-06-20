using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities;

namespace MiraAPI;

public class MiraPluginManager
{
    public Dictionary<Assembly, MiraPluginInfo> RegisteredPlugins = [];

    public void Initialize()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, plugin) =>
        {
            if (!plugin.GetType().GetInterfaces().Contains(typeof(IMiraConfig))) return;
            
            var id = MetadataHelper.GetMetadata(plugin.GetType()).GUID;
            var info = new MiraPluginInfo(id, plugin as IMiraConfig);

            RegisterRoleAttribute(assembly, info);

            ModdedOptionAttribute.Register(assembly);
                
            RegisteredPlugins.Add(assembly, info);

        };
    }

    private static void RegisterRoleAttribute(Assembly assembly, MiraPluginInfo pluginInfo)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRoleAttribute>();
            if (attribute == null) continue;
            
            Helpers.RegisterIl2CppType(type);
            var role = CustomRoleManager.RegisterRole(type, attribute.RoleId);
            
            pluginInfo.CustomRoles.Add((ushort)role.Role, role as ICustomRole);
        }
    }
    
    
    
}
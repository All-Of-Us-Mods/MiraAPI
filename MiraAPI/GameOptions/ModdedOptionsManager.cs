using HarmonyLib;
using MiraAPI.GameOptions.Attributes;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using Reactor.Networking.Rpc;

namespace MiraAPI.GameOptions;

public class ModdedOptionsManager
{
    private static readonly Dictionary<PropertyInfo, ModdedOptionAttribute> OptionAttributes = new();
    private static readonly Dictionary<Type, AbstractOptionGroup> TypeToGroup = new();

    public static readonly Dictionary<uint, IModdedOption> ModdedOptions = new();
    public static readonly List<AbstractOptionGroup> Groups = [];
    
    public static uint NextId => _nextId++;
    private static uint _nextId = 1;
    
    internal static bool RegisterGroup(Type type, MiraPluginInfo pluginInfo)
    {
        if (Activator.CreateInstance(type) is not AbstractOptionGroup group)
        {
            Logger<MiraApiPlugin>.Error($"Failed to create group from {type.Name}");
            return false;
        }

        if (TypeToGroup.ContainsKey(type))
        {
            Logger<MiraApiPlugin>.Error($"Group {type.Name} already exists.");
            return false;
        }

        Groups.Add(group);
        TypeToGroup.Add(type, group);
        pluginInfo.OptionGroups.Add(group);
        
        return true;
    }
    
    internal static void RegisterPropertyOption(Type type, PropertyInfo property, MiraPluginInfo pluginInfo)
    {
        if (!TypeToGroup.TryGetValue(type, out var group))
        {
            Logger<MiraApiPlugin>.Error($"Failed to get group for {type.Name}");
            return;
        }

        if (property.GetValue(group) is not IModdedOption option)
        {
            Logger<MiraApiPlugin>.Error($"Failed to get option for {property.Name}");
            return;
        }
        
        RegisterOption(option, group, pluginInfo);
    }
    
    internal static void RegisterAttributeOption(Type type, ModdedOptionAttribute attribute, PropertyInfo property, MiraPluginInfo pluginInfo)
    {
        if (OptionAttributes.ContainsKey(property))
        {
            Logger<MiraApiPlugin>.Error($"Property {property.Name} already has an attribute registered.");
            return;
        }

        if (!TypeToGroup.TryGetValue(type, out var group))
        {
            Logger<MiraApiPlugin>.Error($"Failed to get group for {type.Name}");
            return;
        }

        var option = attribute.CreateOption(property.GetValue(group), property);
        
        if (option == null)
        {
            Logger<MiraApiPlugin>.Error($"Failed to get option for {property.Name}");
            return;
        }
        
        var setterOriginal = property.GetSetMethod();
        var setterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertySetterPatch));
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(setterOriginal, postfix: new HarmonyMethod(setterPatch));

        var getterOriginal = property.GetGetMethod();
        var getterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertyGetterPatch));
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(getterOriginal, prefix: new HarmonyMethod(getterPatch));

        OptionAttributes.Add(property, attribute);
        attribute.HolderOption = option;
        
        RegisterOption(option, group, pluginInfo);
    }

    private static void RegisterOption(IModdedOption option, AbstractOptionGroup group, MiraPluginInfo pluginInfo)
    {
        option.ParentMod = pluginInfo.MiraPlugin;
        option.AdvancedRole = group.AdvancedRole;
        
        pluginInfo.Options.Add(option);
        
        ModdedOptions.Add(option.Id, option);
        group.Options.Add(option);
    }
    
    internal static void SyncAllOptions(int targetId=-1)
    {
        List<NetData> data = [];
        
        var count = 0;
        foreach (var netData in ModdedOptions.Values.Select(option => option.GetNetData()))
        {
            data.Add(netData);
            count += netData.GetLength();

            if (count <= 1000)
            {
                continue;
            }
            
            Rpc<SyncOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, data.ToArray());
            data.Clear();
            count = 0;
        }
        
        if (data.Count > 0)
        {
            Rpc<SyncOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, data.ToArray());
        }
    }


    internal static void HandleSyncOptions(NetData[] data)
    {
        foreach (var netData in data)
        {
            if (!ModdedOptions.TryGetValue(netData.Id, out var option))
            {
                continue;
            }
            
            Logger<MiraApiPlugin>.Error("Handling option " + option.Title);
            option.HandleNetData(netData.Data);
        }
    }
        
        
    public static void PropertySetterPatch(MethodBase __originalMethod, object value)
    {
        ModdedOptionAttribute attribute = OptionAttributes.First(pair => pair.Key.GetSetMethod().Equals(__originalMethod)).Value;

        attribute?.SetValue(value);
    }

    public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
    {
        ModdedOptionAttribute attribute = OptionAttributes.First(pair => pair.Key.GetGetMethod().Equals(__originalMethod)).Value;

        if (attribute == null)
        {
            return true;
        }
        
        __result = attribute.GetValue();
        return false;
    }
}
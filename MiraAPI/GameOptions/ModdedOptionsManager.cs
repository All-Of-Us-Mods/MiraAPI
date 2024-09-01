using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities;

namespace MiraAPI.GameOptions;

/// <summary>
/// Handles modded options.
/// </summary>
public static class ModdedOptionsManager
{
    private static readonly Dictionary<PropertyInfo, ModdedOptionAttribute> OptionAttributes = new();
    private static readonly Dictionary<Type, AbstractOptionGroup> TypeToGroup = new();

    internal static readonly Dictionary<uint, IModdedOption> ModdedOptions = new();
    internal static readonly List<AbstractOptionGroup> Groups = [];

    internal static uint NextId => _nextId++;
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

        RegisterOption(option, group, property.Name, pluginInfo);
    }

    internal static void RegisterAttributeOption(
        Type type,
        ModdedOptionAttribute attribute,
        PropertyInfo property,
        MiraPluginInfo pluginInfo)
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

        RegisterOption(option, group, property.Name, pluginInfo);
    }

    private static void RegisterOption(
        IModdedOption option,
        AbstractOptionGroup group,
        string propertyName,
        MiraPluginInfo pluginInfo)
    {
        var groupName = group.GetType().FullName;

        option.ConfigDefinition = new ConfigDefinition(groupName, propertyName);

        option.ParentMod = pluginInfo.MiraPlugin;
        option.AdvancedRole = group.AdvancedRole;

        pluginInfo.Options.Add(option);

        ModdedOptions.Add(option.Id, option);
        group.Options.Add(option);
    }

    internal static void SyncAllOptions(int targetId = -1)
    {
        var chunks = ModdedOptions.Values.Select(option => option.GetNetData()).ChunkNetData(1000);

        while (chunks.Count > 0)
        {
            Rpc<SyncOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, chunks.Dequeue());
        }
    }

    internal static void HandleSyncOptions(NetData[] data)
    {
        // necessary to disable then re-enable this setting
        // we dont know how other plugins handle their configs
        // this way, all the options are saved at once, instead of one by one
        var oldConfigSetting = new Dictionary<MiraPluginInfo, bool>();
        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins())
        {
            oldConfigSetting.Add(plugin, plugin.PluginConfig.SaveOnConfigSet);
            plugin.PluginConfig.SaveOnConfigSet = false;
        }

        foreach (var netData in data)
        {
            if (!ModdedOptions.TryGetValue(netData.Id, out var option))
            {
                continue;
            }

            option.HandleNetData(netData.Data);
        }

        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins())
        {
            plugin.PluginConfig.Save();
            plugin.PluginConfig.SaveOnConfigSet = oldConfigSetting[plugin];
        }

        if (LobbyInfoPane.Instance)
        {
            LobbyInfoPane.Instance.RefreshPane();
        }
    }

    /// <summary>
    /// Patches the setter of a property to update the value of the option.
    /// </summary>
    /// <param name="__originalMethod">The original setter method.</param>
    /// <param name="value">The new object value.</param>
    public static void PropertySetterPatch(MethodBase __originalMethod, object value)
    {
        var attribute = OptionAttributes.First(pair => pair.Key.GetSetMethod() == __originalMethod).Value;
        attribute.SetValue(value);
    }

    /// <summary>
    /// Patches the getter of a property to return the value of the option.
    /// </summary>
    /// <param name="__originalMethod">The original getter method.</param>
    /// <param name="__result">The result of the property getter.</param>
    /// <returns>False so the original getter gets skipped.</returns>
    public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
    {
        var attribute = OptionAttributes.First(pair => pair.Key.GetGetMethod() == __originalMethod).Value;
        __result = attribute.GetValue();
        return false;
    }
}

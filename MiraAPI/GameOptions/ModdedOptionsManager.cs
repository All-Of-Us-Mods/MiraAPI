using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace MiraAPI.GameOptions;

/// <summary>
/// Handles <see cref="IModdedOption"/>s.
/// </summary>
public static class ModdedOptionsManager
{
    private static readonly Dictionary<PropertyInfo, ModdedOptionAttribute> OptionAttributes = [];
    private static readonly Dictionary<Type, AbstractOptionGroup> TypeToGroup = [];

    internal static readonly Dictionary<OptionBehaviour, ModdedPlayerOption> CreatedPlayerOptions = [];
    internal static readonly Dictionary<uint, IModdedOption> ModdedOptions = [];
    internal static readonly List<AbstractOptionGroup> Groups = [];

    internal static uint NextId => _nextId++;
    private static uint _nextId = 1;

    /// <summary>
    /// Pops a message notification of a setting being changed.
    /// </summary>
    /// <param name="notif">The game's notification popper.</param>
    /// <param name="key">The <see cref="StringNames"/> key of the option that changed.</param>
    /// <param name="value">The new value of the option.</param>
    /// <param name="textColor">The color of the notification.</param>
    /// <param name="sprite">The optional sprite of the notification.</param>
    /// <param name="playSound">A flag that indicates if a notification sound should be played.</param>
    public static void AddSettingsChangeMessage(NotificationPopper notif, StringNames key, string value, Color textColor, TMP_SpriteAsset? sprite, bool playSound = true)
    {
        string item;
        var text = textColor.ToTextColor();
        item = sprite != null
            ? TranslationController.Instance.GetString(
                StringNames.LobbyChangeSettingNotification,
                string.Concat(
                    "<sprite name=\"",
                    sprite.name,
                    "\"><font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
                    text,
                    TranslationController.Instance.GetString(key),
                    "</color></font>"),
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + value + "</font>"
            )
            : TranslationController.Instance.GetString(
                StringNames.LobbyChangeSettingNotification,
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" +
                text +
                TranslationController.Instance.GetString(key) + "</color></font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + value + "</font>");
        notif.SettingsChangeMessageLogic(key, item, playSound);
    }

    [SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Dynamic singleton initialization requires reflection to bypass the private field access modifier because the type is only known at runtime."
    )]
    internal static bool RegisterGroup(Type type, MiraPluginInfo pluginInfo)
    {
        if (Activator.CreateInstance(type) is not AbstractOptionGroup group)
        {
            return false;
        }

        if (TypeToGroup.ContainsKey(type))
        {
            Error($"Group {type.Name} already exists.");
            return false;
        }

        Groups.Add(group);
        TypeToGroup.Add(type, group);
        pluginInfo.InternalOptionGroups.Add(group);

        typeof(OptionGroupSingleton<>).MakeGenericType(type)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)! // Message suppression points to here
            .SetValue(null, group);

        return true;
    }

    internal static void RegisterPropertyOption(Type type, PropertyInfo property, MiraPluginInfo pluginInfo)
    {
        if (!TypeToGroup.TryGetValue(type, out var group))
        {
            Error($"Failed to get group for {type.Name}");
            return;
        }

        if (property.GetValue(group) is not IModdedOption option)
        {
            Error($"Failed to get option for {property.Name}");
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
            Error($"Property {property.Name} already has an attribute registered.");
            return;
        }

        if (!TypeToGroup.TryGetValue(type, out var group))
        {
            Error($"Failed to get group for {type.Name}");
            return;
        }

        var option = attribute.CreateOption(property.GetValue(group), property);

        if (option == null)
        {
            Error($"Failed to get option for {property.Name}");
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

    internal static void RegisterOption(
        IModdedOption option,
        AbstractOptionGroup group,
        string propertyName,
        MiraPluginInfo pluginInfo)
    {
        var groupName = group.GetType().FullName;

        option.ConfigDefinition = new ConfigDefinition(groupName, propertyName);

        option.ParentGroup = group;
        option.ParentMod = pluginInfo.MiraPlugin;
        pluginInfo.InternalOptions.Add(option);
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
        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
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

        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
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
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention.")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Harmony naming convention.")]
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
    /// <returns><see langword="false"/> so the original getter gets skipped.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Harmony naming convention.")]
    public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
    {
        var attribute = OptionAttributes.First(pair => pair.Key.GetGetMethod() == __originalMethod).Value;
        __result = attribute.GetValue();
        return false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MiraAPI.GameModes;
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
    private static readonly Dictionary<PropertyInfo, PropertyOptionAttribute> OptionAttributes = [];
    private static readonly Dictionary<Type, AbstractOptionGroup> TypeToGroup = [];

    internal static readonly Dictionary<OptionBehaviour, ModdedPlayerOption> CreatedPlayerOptions = [];
    internal static readonly Dictionary<OptionBehaviour, ModdedStringOption> CreatedStringOptions = [];
    internal static readonly Dictionary<Type, List<AbstractOptionGroup>> GameModeOptionGroups = [];
    internal static readonly Dictionary<uint, IModdedOption> ModdedOptions = [];
    internal static readonly List<AbstractOptionGroup> Groups = [];

    internal static uint NextId => _nextId++;
    private static uint _nextId = 1;

    public static void AddSettingsChangeMessage(NotificationPopper notif, StringNames key, string value, Color textColor, TMP_SpriteAsset? sprite, bool playSound = true)
    {
        string item;
        var text = textColor.ToTextColor();
        if (sprite != null)
        {
            item = TranslationController.Instance.GetString(
                StringNames.LobbyChangeSettingNotification,
                string.Concat(
                    "<sprite name=\"",
                    sprite.name,
                    "\"><font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
                    text,
                    TranslationController.Instance.GetString(key),
                    "</color></font>"),
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + value + "</font>"
            );
        }
        else
        {
            item = TranslationController.Instance.GetString(
                StringNames.LobbyChangeSettingNotification,
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" +
                text +
                TranslationController.Instance.GetString(key) + "</color></font>",
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + value + "</font>");
        }
        notif.SettingsChangeMessageLogic(key, item, playSound);
    }

    internal static bool RegisterGroup(Type type)
    {
        if (Activator.CreateInstance(type) is not AbstractOptionGroup group)
        {
            return false;
        }

        if (TypeToGroup.ContainsKey(type))
        {
            Logger<MiraApiPlugin>.Error($"Group {type.Name} already exists.");
            return false;
        }

        Groups.Add(group);
        TypeToGroup.Add(type, group);

        typeof(OptionGroupSingleton<>).MakeGenericType(type)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, group);

        return true;
    }

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
        if (group.OptionableType?.IsAssignableTo(typeof(AbstractGameMode)) == true)
        {
            if (GameModeOptionGroups.TryGetValue(group.OptionableType, out var oldList))
            {
                oldList.Add(group);
            }
            else
            {
                GameModeOptionGroups.Add(group.OptionableType, new List<AbstractOptionGroup>() { group });
            }
        }
        pluginInfo.InternalOptionGroups.Add(group);

        typeof(OptionGroupSingleton<>).MakeGenericType(type)
#pragma warning disable S3011
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
#pragma warning restore S3011
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

        var visibilityAttr = property.GetCustomAttribute<ModdedOptionVisiblityAttribute>();
        var visibilityFunc = visibilityAttr?.GetVisibility(group, property);
        if (visibilityFunc != null)
        {
            option.Visible = visibilityFunc;
        }

        RegisterOption(option, group, property.Name, pluginInfo);
    }

    internal static void RegisterPropertyOptionList(Type type, PropertyInfo property, MiraPluginInfo pluginInfo)
    {
        if (!TypeToGroup.TryGetValue(type, out var group))
        {
            Error($"Failed to get group for {type.Name}");
            return;
        }

        if (property.GetValue(group) is not IModdedOptionList optionList)
        {
            Error($"Failed to get option list for {property.Name}");
            return;
        }

        for (int i = 0; i < optionList.Count; i++)
        {
            RegisterOption(optionList[i], group, property.Name + i, pluginInfo);
        }
    }

    internal static void RegisterAttributeOptionList(
        Type type,
        ModdedOptionListAttribute attribute,
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

        var propertyVal = property.GetValue(group);

        if (propertyVal == null)
        {
            Error("Cannot initialize option list with null value.");
            return;
        }

        var propertyList = (IList)propertyVal;
        var optionList = attribute.CreateOptionList(propertyList, property);

        if (optionList == null)
        {
            Error($"Failed to get option for {property.Name}");
            return;
        }
        if (propertyList.Count != optionList.Count)
        {
            Error("Mismatch in count between values and created options.");
            return;
        }

        var setterOriginal = property.GetSetMethod();
        var setterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertySetterPatch));
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(setterOriginal, postfix: new HarmonyMethod(setterPatch));

        var getterOriginal = property.GetGetMethod();
        var getterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertyGetterPatch));
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(getterOriginal, prefix: new HarmonyMethod(getterPatch));

        var listIndex = propertyList.GetType().GetProperty("Item")!;

        var listSetterOriginal = listIndex.GetSetMethod();
        var listSetterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertyListSetterPatch));
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(listSetterOriginal, postfix: new HarmonyMethod(listSetterPatch));

        var listGetterOriginal = listIndex.GetSetMethod();
        var listGetterPatch = typeof(ModdedOptionsManager).GetMethod(nameof(PropertyListGetterPatch));
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(listGetterOriginal, prefix: new HarmonyMethod(listGetterPatch));

        OptionAttributes.Add(property, attribute);
        attribute.HolderOptionList = optionList;
        attribute.Value = propertyVal;

        var visibilityAttr = property.GetCustomAttribute<ModdedOptionVisiblityAttribute>();
        var visibilityFunc = visibilityAttr?.GetListVisibility(group, property);
        for (int i = 0; i < optionList.Count; i++)
        {
            var option = optionList[i];
            if (visibilityFunc != null)
            {
                option.Visible = () => visibilityFunc(i);
            }
            RegisterOption(option, group, property.Name + i, pluginInfo);
        }
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
#pragma warning disable CA1707
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    public static void PropertySetterPatch(MethodBase __originalMethod, object value)
#pragma warning restore CA1707
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
#pragma warning disable CA1707
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
#pragma warning restore CA1707
    {
        var attribute = OptionAttributes.First(pair => pair.Key.GetGetMethod() == __originalMethod).Value;
        __result = attribute.GetValue();
        return false;
    }

    /// <summary>
    /// Patches the setter of a list property to update the value of the option.
    /// </summary>
    /// <param name="__instance">The list's instance.</param>
    /// <param name="index">The index to find in the list.</param>
    /// <param name="value">The new object value.</param>
#pragma warning disable CA1707
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    public static void PropertyListSetterPatch(object __instance, int index, object value)
#pragma warning restore CA1707
    {
        var attribute = (ModdedOptionListAttribute)OptionAttributes.First(
            pair => pair.Value is ModdedOptionListAttribute list && ReferenceEquals(list.Value, __instance)).Value;
        attribute.SetValue(index, value);
    }

    /// <summary>
    /// Patches the getter of a list property to return the value of the option.
    /// </summary>
    /// <param name="__instance">The list's instance.</param>
    /// <param name="index">The index to find in the list.</param>
    /// <param name="__result">The result of the property getter.</param>
    /// <returns>False so the original getter gets skipped.</returns>
#pragma warning disable CA1707
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    public static bool PropertyListGetterPatch(object __instance, int index, ref object __result)
#pragma warning restore CA1707
    {
        var attribute = (ModdedOptionListAttribute)OptionAttributes.First(
            pair => pair.Value is ModdedOptionListAttribute list && ReferenceEquals(list.Value, __instance)).Value;
        __result = attribute.GetValue(index);
        return false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using HarmonyLib;
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
    private static readonly Dictionary<MethodBase, PropertyOptionAttribute> GetterLookups = [];
    private static readonly Dictionary<MethodBase, PropertyOptionAttribute> SetterLookups = [];
    private static readonly Dictionary<Type, AbstractOptionGroup> TypeToGroup = [];
    private static readonly HashSet<PropertyInfo> OptionAttributes = [];

    private static readonly HarmonyMethod CachedGetterPatch = new(typeof(ModdedOptionsManager), nameof(PropertyGetterPatch));
    private static readonly HarmonyMethod CachedSetterPatch = new(typeof(ModdedOptionsManager), nameof(PropertySetterPatch));

    internal static readonly ConditionalWeakTable<object, ModdedOptionListAttribute> ListInstanceCache = [];
    internal static readonly Dictionary<OptionBehaviour, ModdedPlayerOption> CreatedPlayerOptions = [];
    internal static readonly Dictionary<OptionBehaviour, ModdedStringOption> CreatedStringOptions = [];
    internal static readonly Dictionary<Type, List<AbstractOptionGroup>> GameModeOptionGroups = [];
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
        var text = textColor.ToTextColor();
        var translatedKey = TranslationController.Instance.GetString(key);
        var fontTag = "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">";
        var spritePrefix = sprite != null ? $"<sprite name=\"{sprite.name}\">" : string.Empty;

        var item = TranslationController.Instance.GetString(
            StringNames.LobbyChangeSettingNotification,
            $"{spritePrefix}{fontTag}{text}{translatedKey}</color></font>",
            $"{fontTag}{value}</font>");
        notif.SettingsChangeMessageLogic(key, item, playSound);
    }

    internal static bool RegisterGroup(Type type)
    {
        if (TypeToGroup.ContainsKey(type))
        {
            Error($"Group {type.Name} already exists.");
            return false;
        }

        if (Activator.CreateInstance(type) is not AbstractOptionGroup group)
        {
            return false;
        }

        RegisterGroupInternal(group, type);
        return true;
    }

    internal static bool RegisterGroup(Type type, MiraPluginInfo pluginInfo)
    {
        if (TypeToGroup.ContainsKey(type))
        {
            Error($"Group {type.Name} already exists.");
            return false;
        }

        if (Activator.CreateInstance(type) is not AbstractOptionGroup group)
        {
            return false;
        }

        if (typeof(AbstractGameMode).IsAssignableFrom(group.OptionableType))
        {
            if (GameModeOptionGroups.TryGetValue(group.OptionableType, out var oldList))
            {
                oldList.Add(group);
            }
            else
            {
                GameModeOptionGroups.Add(group.OptionableType, [group]);
            }
        }

        pluginInfo.InternalOptionGroups.Add(group);

        RegisterGroupInternal(group, type);
        return true;
    }

    [SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Dynamic singleton initialization requires reflection to bypass the private field access modifier because the type is only known at runtime.")]
    private static void RegisterGroupInternal(AbstractOptionGroup group, Type type)
    {
        Groups.Add(group);
        TypeToGroup.Add(type, group);
        typeof(OptionGroupSingleton<>).MakeGenericType(type)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, group);
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
        if (!OptionAttributes.Add(property))
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

        var getterOriginal = property.GetGetMethod(true)!;
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(getterOriginal, prefix: CachedGetterPatch);
        GetterLookups[getterOriginal] = attribute;

        var setterOriginal = property.GetSetMethod(true);

        if (setterOriginal != null)
        {
            PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(setterOriginal, postfix: CachedSetterPatch);
            SetterLookups[setterOriginal] = attribute;
        }

        attribute.HolderOption = option;
        attribute.ValueType = property.PropertyType;

        var customAttributes = property.GetCustomAttributes(false);

        foreach (var attr in customAttributes)
        {
            switch (attr)
            {
                case ModdedOptionVisibilityAttribute visibilityAttr:
                    option.Visible = visibilityAttr.GetVisibility(group, property) ?? DelegateUtilities.AlwaysTrue;
                    break;
                case ModdedOptionGetterAttribute getterAttr:
                    option.Getter = getterAttr.GetGetter(group, property, option);
                    break;
                case ModdedOptionSetterAttribute setterAttr:
                    option.Setter = setterAttr.GetSetter(group, property, option);
                    break;
                case ModdedOptionChangedAttribute changedAttr:
                    option.ChangedEvent = changedAttr.GetChanged(group, property, option);
                    break;
            }
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

        if (property.GetValue(group) is not IBaseModdedOptionList optionList)
        {
            Error($"Failed to get option list for {property.Name}");
            return;
        }

        var configName = optionList.ConfigName ?? (i => property.Name + (i + 1));

        for (var i = 0; i < optionList.Count; i++)
        {
            RegisterOption(optionList.ElementAt(i), group, configName(i), pluginInfo);
        }
    }

    internal static void RegisterAttributeOptionList(
        Type type,
        ModdedOptionListAttribute attribute,
        PropertyInfo property,
        MiraPluginInfo pluginInfo)
    {
        if (!OptionAttributes.Add(property))
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

        var listType = propertyList.GetType();
        var valueType = listType.GetGenericArguments()[0];

        if (!listType.IsGenericType || listType.GetGenericTypeDefinition() != typeof(ModdedArray<>))
        {
            Error($"Please use ModdedList<{valueType.Name}> for modded option lists.");
            return;
        }

        var getterOriginal = property.GetGetMethod(true)!;
        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(getterOriginal, prefix: CachedGetterPatch);
        GetterLookups[getterOriginal] = attribute;

        var setterOriginal = property.GetSetMethod(true);

        if (setterOriginal != null)
        {
            PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(setterOriginal, postfix: CachedSetterPatch);
            SetterLookups[setterOriginal] = attribute;
        }

        attribute.HolderOptionList = optionList;
        attribute.Value = propertyList;
        attribute.ValueType = valueType;

        ListInstanceCache.Add(propertyVal, attribute);

        var customAttributes = property.GetCustomAttributes(false);

        Func<int, Delegate?>? visibilityFactory = null, getterFactory = null, setterFactory = null, changedFactory = null;

        foreach (var attr in customAttributes)
        {
            switch (attr)
            {
                case ModdedOptionVisibilityAttribute visibilityAttr:
                    visibilityFactory = visibilityAttr.GetListVisibility(group, property);
                    break;
                case ModdedOptionGetterAttribute getterAttr:
                    getterFactory = getterAttr.GetListGetter(group, property, valueType);
                    break;
                case ModdedOptionSetterAttribute setterAttr:
                    setterFactory = setterAttr.GetListSetter(group, property, valueType);
                    break;
                case ModdedOptionChangedAttribute changedAttr:
                    changedFactory = changedAttr.GetListChanged(group, property, valueType);
                    break;
            }
        }

        for (var i = 0; i < optionList.Count; i++)
        {
            var option = optionList.ElementAt(i);

            if (visibilityFactory != null)
                option.Visible = (Func<bool>)(visibilityFactory(i) ?? DelegateUtilities.AlwaysTrue);

            if (getterFactory != null)
                option.Getter = getterFactory(i);

            if (setterFactory != null)
                option.Setter = setterFactory(i);

            if (changedFactory != null)
                option.ChangedEvent = changedFactory(i);

            RegisterOption(option, group, property.Name + (i + 1), pluginInfo);
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
            Rpc<SyncOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, new MiraOptionNetData(false, chunks.Dequeue()));
        }
    }

    internal static void HandleSyncOptions(bool showNotifs, NetData[] data)
    {
        // necessary to disable then re-enable this setting
        // we don't know how other plugins handle their configs
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

            option.ShowNotification = showNotifs;
            option.HandleNetData(netData.Data);
        }

        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
        {
            plugin.PluginConfig.Save();
            plugin.PluginConfig.SaveOnConfigSet = oldConfigSetting[plugin];
        }

        if (LobbyInfoPane.InstanceExists)
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
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Read above.")]
    public static void PropertySetterPatch(MethodBase __originalMethod, object value)
    {
        if (!SetterLookups.TryGetValue(__originalMethod, out var attribute))
            return;

        attribute.SetValue(value);

        if (attribute is ModdedOptionListAttribute listAttribute && value != null)
            ListInstanceCache.AddOrUpdate(value, listAttribute);
    }

    /// <summary>
    /// Patches the getter of a property to return the value of the option.
    /// </summary>
    /// <param name="__originalMethod">The original getter method.</param>
    /// <param name="__result">The result of the property getter.</param>
    /// <returns><see langword="false"/> so the original getter gets skipped.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Read above.")]
    public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
    {
        if (!GetterLookups.TryGetValue(__originalMethod, out var attribute))
            return true;

        __result = attribute.GetValue();
        return false;
    }
}

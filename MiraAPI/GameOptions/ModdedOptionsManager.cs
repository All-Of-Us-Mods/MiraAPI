using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.GameOptions.Attributes;
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
    private static readonly Dictionary<RuntimeMethodHandle, PropertyOptionAttribute> GetterLookup = [];
    private static readonly Dictionary<RuntimeMethodHandle, PropertyOptionAttribute> SetterLookup = [];
    private static readonly HashSet<PropertyInfo> OptionAttributes = [];
    private static readonly HashSet<Type> Types = [];

    private static readonly MethodInfo OpenGetterPatch = typeof(ModdedOptionsManager)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == nameof(PropertyGetterPatch) && m.IsGenericMethodDefinition);
    private static readonly MethodInfo OpenSetterPatch = typeof(ModdedOptionsManager)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == nameof(PropertySetterPatch) && m.IsGenericMethodDefinition);
    private static readonly Dictionary<RuntimeTypeHandle, HarmonyMethod> ConcretisedGetters = [];
    private static readonly Dictionary<RuntimeTypeHandle, HarmonyMethod> ConcretisedSetters = [];

    private static readonly Type OptionType = typeof(IModdedOption);
    private static readonly Type OptionListType = typeof(IModdedOptionList);

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

    [SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Dynamic singleton initialization requires reflection to bypass the private field access modifier because the type is only known at runtime.")]
    internal static bool RegisterGroup(Type type, MiraPluginInfo pluginInfo)
    {
        if (!type.IsAssignableTo(typeof(AbstractOptionGroup)))
        {
            return false;
        }

        if (!Types.Add(type))
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

        Groups.Add(group);

        typeof(OptionGroupSingleton<>).MakeGenericType(type)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)! // Suppression refers to this
            .SetValue(null, group);

        foreach (var property in type.GetProperties(MiraPluginManager.DeclaredInstanceMembers))
        {
            var propType = property.PropertyType;

            if (propType.IsAssignableTo(OptionType))
            {
                RegisterPropertyOption(type, property, pluginInfo, group);
                continue;
            }

            if (propType.IsAssignableTo(OptionListType))
            {
                RegisterPropertyOptionList(type, property, pluginInfo, group);
                continue;
            }

            var attributes = property.GetCustomAttributes(false);

            foreach (var attr in attributes)
            {
                if (attr is ModdedOptionAttribute moddedAttr)
                {
                    RegisterAttributeOption(type, moddedAttr, property, pluginInfo, propType, attributes, group);
                    break;
                }

                if (attr is ModdedOptionListAttribute listAttr)
                {
                    RegisterAttributeOptionList(type, listAttr, property, pluginInfo, propType, attributes, group);
                    break;
                }
            }
        }

        if (!pluginInfo.DebugMode)
            return true;

        foreach (var field in type.GetFields(MiraPluginManager.DeclaredInstanceMembers)
            .Where(f => !MiraPluginManager.IsAutoPropertyBackingField(f)
                && (f.FieldType.IsAssignableTo(typeof(IModdedOption)) || f.FieldType.IsAssignableTo(typeof(IModdedOptionList)))))
        {
            Error($"{field.Name} is a field, not a property. Use properties for options.");
        }

        return true;
    }

    private static void PatchProperty(PropertyInfo property, Type propType, PropertyOptionAttribute attribute)
    {
        var handle = propType.TypeHandle;
        ApplyPatch(handle, propType, ConcretisedGetters, GetterLookup, property.GetGetMethod(true), OpenGetterPatch, attribute);
        ApplyPatch(handle, propType, ConcretisedSetters, SetterLookup, property.GetSetMethod(true), OpenSetterPatch, attribute);
    }

    private static void ApplyPatch(
        RuntimeTypeHandle handle,
        Type propType,
        Dictionary<RuntimeTypeHandle, HarmonyMethod> concretised,
        Dictionary<RuntimeMethodHandle, PropertyOptionAttribute> lookup,
        MethodInfo? toPatch,
        MethodInfo patch,
        PropertyOptionAttribute attribute)
    {
        if (toPatch == null)
            return;

        if (!concretised.TryGetValue(handle, out var concrete))
            concretised[handle] = concrete = new(patch.MakeGenericMethod(propType));

        PluginSingleton<MiraApiPlugin>.Instance.Harmony.Patch(toPatch, prefix: concrete);
        lookup[toPatch.MethodHandle] = attribute;
    }

    private static void RegisterPropertyOption(
        Type type,
        PropertyInfo property,
        MiraPluginInfo pluginInfo,
        AbstractOptionGroup group)
    {
        if (property.GetValue(group) is not IModdedOption option)
        {
            Error($"Failed to get option for {property.Name}");
            return;
        }

        RegisterOption(type, option, group, property.Name, pluginInfo);
    }

    private static void RegisterAttributeOption(
        Type type,
        ModdedOptionAttribute attribute,
        PropertyInfo property,
        MiraPluginInfo pluginInfo,
        Type propType,
        object[] attributes,
        AbstractOptionGroup group)
    {
        if (!OptionAttributes.Add(property))
        {
            Error($"Property {property.Name} already has an attribute registered.");
            return;
        }

        var option = attribute.CreateOption(property.GetValue(group), property, group);

        if (option == null)
        {
            Error($"Failed to get option for {property.Name}");
            return;
        }

        PatchProperty(property, propType, attribute);

        attribute.HolderOption = option;
        attribute.ValueType = propType;

        foreach (var attr in attributes)
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

        RegisterOption(type, option, group, property.Name, pluginInfo);
    }

    private static void RegisterPropertyOptionList(
        Type type,
        PropertyInfo property,
        MiraPluginInfo pluginInfo,
        AbstractOptionGroup group)
    {
        if (property.GetValue(group) is not IModdedOptionList optionList)
        {
            Error($"Failed to get option list for {property.Name}");
            return;
        }

        var configName = optionList.ConfigName ?? (i => property.Name + (i + 1));

        for (var i = 0; i < optionList.Count; i++)
        {
            RegisterOption(type, optionList[i], group, configName(i), pluginInfo);
        }
    }

    private static void RegisterAttributeOptionList(
        Type type,
        ModdedOptionListAttribute attribute,
        PropertyInfo property,
        MiraPluginInfo pluginInfo,
        Type propType,
        object[] attributes,
        AbstractOptionGroup group)
    {
        if (!OptionAttributes.Add(property))
        {
            Error($"Property {property.Name} already has an attribute registered.");
            return;
        }

        if (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(ModdedArray<>))
        {
            var typeName = propType.IsGenericType ? propType.GetGenericArguments()[0].Name : "T";
            Error($"Please use ModdedArray<{typeName}> for modded option lists.");
            return;
        }

        var propertyVal = property.GetValue(group);

        if (propertyVal == null)
        {
            Error("Cannot initialize option list with null value.");
            return;
        }

        var list = (IList)propertyVal;

        attribute.Titles = new string[list.Count];

        var optionList = attribute.CreateOptionList(list, property, group);

        if (optionList == null)
        {
            Error($"Failed to get option for {property.Name}");
            return;
        }

        PatchProperty(property, propType, attribute);

        var valueType = propType.GetGenericArguments()[0];

        attribute.HolderOptionList = optionList;
        attribute.Value = list;
        attribute.ValueType = valueType;

        ((IModdedArrayBinding)propertyVal).Bind(attribute);

        Func<int, Delegate?>? visibilityFactory = null, getterFactory = null, setterFactory = null, changedFactory = null;

        foreach (var attr in attributes)
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
            var option = optionList[i];

            if (visibilityFactory != null)
                option.Visible = visibilityFactory(i) as Func<bool> ?? DelegateUtilities.AlwaysTrue;

            if (getterFactory != null)
                option.Getter = getterFactory(i);

            if (setterFactory != null)
                option.Setter = setterFactory(i);

            if (changedFactory != null)
                option.ChangedEvent = changedFactory(i);

            RegisterOption(type, option, group, property.Name + (i + 1), pluginInfo);
        }
    }

    private static void RegisterOption(
        Type type,
        IModdedOption option,
        AbstractOptionGroup group,
        string propertyName,
        MiraPluginInfo pluginInfo)
    {
        option.ConfigDefinition = new ConfigDefinition(type.FullName, propertyName);
        option.ParentGroup = group;
        option.ParentMod = pluginInfo.MiraPlugin;

        pluginInfo.InternalOptions.Add(option);
        ModdedOptions.Add(option.Id, option);
        group.Options.Add(option);
    }

    internal static void SyncAllOptions(int targetId = -1)
    {
        var chunks = ModdedOptions.Values.Select(option => option.GetNetData()).ChunkNetData(1000);

        while (chunks.TryDequeue(out var netData))
        {
            Rpc<SyncGameOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, new MiraOptionNetData(false, netData));
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
    [Obsolete("Use PropertySetterPatch<T> instead. This will be removed in a future version.")]
    public static void PropertySetterPatch(MethodBase __originalMethod, object value)
    {
        if (!SetterLookup.TryGetValue(__originalMethod.MethodHandle, out var attribute))
            return;

        attribute.SetValue(value);

        if (attribute is ModdedOptionListAttribute listAttribute && value is IModdedArrayBinding binding)
            binding.Bind(listAttribute);
    }

    /// <summary>
    /// Patches the setter of a property to update the value of the option.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="__originalMethod">The original setter method.</param>
    /// <param name="value">The new object value.</param>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention.")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Read above.")]
    public static void PropertySetterPatch<T>(MethodBase __originalMethod, T value)
    {
        if (!SetterLookup.TryGetValue(__originalMethod.MethodHandle, out var attribute))
            return;

        if (attribute is ITypedOption<T> typedAttribute)
            typedAttribute.SetValueUnboxed(value);
        else
            attribute.SetValue(value!);

        if (attribute is ModdedOptionListAttribute listAttribute && value is IModdedArrayBinding binding)
            binding.Bind(listAttribute);
    }

    /// <summary>
    /// Patches the getter of a property to return the value of the option.
    /// </summary>
    /// <param name="__originalMethod">The original getter method.</param>
    /// <param name="__result">The result of the property getter.</param>
    /// <returns><see langword="false"/> so the original getter gets skipped.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Read above.")]
    [Obsolete("Use PropertyGetterPatch<T> instead. This will be removed in a future version.")]
    public static bool PropertyGetterPatch(MethodBase __originalMethod, ref object __result)
    {
        if (!GetterLookup.TryGetValue(__originalMethod.MethodHandle, out var attribute))
            return true;

        __result = attribute.GetValue();
        return false;
    }

    /// <summary>
    /// Patches the getter of a property to return the value of the option.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="__originalMethod">The original getter method.</param>
    /// <param name="__result">The result of the property getter.</param>
    /// <returns><see langword="false"/> so the original getter gets skipped.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony naming convention")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Read above.")]
    public static bool PropertyGetterPatch<T>(MethodBase __originalMethod, ref T __result)
    {
        if (!GetterLookup.TryGetValue(__originalMethod.MethodHandle, out var attribute))
            return true;

        if (attribute is ITypedOption<T> typedAttribute)
            __result = typedAttribute.GetValueUnboxed();
        else
            __result = (T)attribute.GetValue();

        return false;
    }
}

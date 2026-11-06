using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.Colors;
using MiraAPI.Events;
using MiraAPI.GameEnd;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using MiraAPI.Presets;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Networking;

namespace MiraAPI.PluginLoading;

/// <summary>
/// Mira Plugin manager.
/// </summary>
[SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Intended behaviour.")]
public sealed class MiraPluginManager
{
    private MiraPluginManager()
    {
    }

    private const BindingFlags DeclaredInstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    private const BindingFlags DeclaredStaticMembers = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private readonly Dictionary<Assembly, MiraPluginInfo> _registeredPlugins = [];

    internal MiraPluginInfo[] RegisteredPlugins { get; private set; } = [];
    internal MiraPluginInfo[] RegisteredPluginsWithOptions { get; private set; } = [];
    internal MiraPluginInfo[] PluginsWithOptionsOrGameModes { get; private set; } = [];

    internal Dictionary<MiraPluginInfo, List<Type>> QueuedRoleRegistrations { get; } = [];
    internal static MiraPluginManager Instance { get; } = new();
    internal static ConfigFile MiraApiConfig { get; private set; } = null!;
    internal static MiraPluginInfo MiraApiPluginInfo { get; private set; } = null!;

    internal void Initialize(BasePlugin miraApiPlugin, IMiraPlugin apiPlugin)
    {
        CustomGameModeManager.RegisterDefaultMode();
        CustomGameModeManager.GetAndSetGameMode();

        if (IL2CPPChainloader.Instance.Plugins.TryGetValue(MiraApiPlugin.Id, out var newPlugin))
        {
            var registrationTimer = Stopwatch.StartNew();
            var info = RegisterPlugin(typeof(MiraPluginManager).Assembly, apiPlugin, newPlugin, miraApiPlugin, true);

            MiraApiConfig = info.PluginConfig;
            MiraApiPluginInfo = info;

            registrationTimer.Stop();
            Info($"Registered Mira API as mod in {registrationTimer.ElapsedMilliseconds}ms.");
        }

        IL2CPPChainloader.Instance.PluginLoad += (pluginInfo, assembly, plugin) =>
        {
            if (plugin is not IMiraPlugin miraPlugin || pluginInfo.Metadata.GUID == MiraApiPlugin.Id)
            {
                return;
            }

            var registrationTimer = Stopwatch.StartNew();
            RegisterPlugin(assembly, miraPlugin, pluginInfo, plugin, false);
            registrationTimer.Stop();
            Info($"Registered mod {pluginInfo.Metadata.Name} with Mira API in {registrationTimer.ElapsedMilliseconds}ms.");
        };

        IL2CPPChainloader.Instance.Finished += () =>
        {
            PaletteManager.RegisterAllColors();

            // Save all buttons into a read-only collection for easy access
            CustomButtonManager.Buttons = new ReadOnlyCollection<CustomActionButton>(CustomButtonManager.CustomButtons);
            CustomButtonManager.EventTypes = new ReadOnlyDictionary<Type, Type>(CustomButtonManager.ButtonEventTypes);
            CustomButtonManager.CancelledEventTypes = new ReadOnlyDictionary<Type, Type>(CustomButtonManager.ButtonCancelledEventTypes);

            // Cache all the registered plugins into an array for easy access
            RegisteredPlugins = [.. _registeredPlugins.Values];
            RegisteredPluginsWithOptions = [.. RegisteredPlugins.Where(m => m.MiraPlugin.DisplayOnOptionsMenu)];
            PluginsWithOptionsOrGameModes = [.. RegisteredPlugins.Where(m => m.MiraPlugin.DisplayOnOptionsMenu || m.GameModes.Count > 0)];

            ModifierManager.Modifiers = new ReadOnlyCollection<IBaseModifier>(ModifierManager.InternalModifiers);
        };
    }

    private MiraPluginInfo RegisterPlugin(Assembly assembly, IMiraPlugin plugin, PluginInfo pluginInfo, BasePlugin basePlugin, bool isMira)
    {
        var info = new MiraPluginInfo(plugin, pluginInfo);
        var roles = new List<Type>();

        var oldConfigSetting = info.PluginConfig.SaveOnConfigSet;
        info.PluginConfig.SaveOnConfigSet = false;

        ProcessAssemblyTypes(assembly, info, basePlugin, plugin, roles, isMira);

        info.PluginConfig.Save();
        info.PluginConfig.SaveOnConfigSet = oldConfigSetting;

        info.InternalOptionGroups.Sort((x, y) => x.GroupPriority.CompareTo(y.GroupPriority));
        QueuedRoleRegistrations.Add(info, roles);
        _registeredPlugins.Add(assembly, info);

        info.SavePublicCollections();
        PresetManager.CreateDefaultPreset(info);
        PresetManager.LoadPresets(info);

        return info;
    }

    private static bool IsAutoPropertyBackingField(FieldInfo field)
    {
        return field.IsDefined<CompilerGeneratedAttribute>(false) &&
                field.Name.Contains(">k__BackingField", StringComparison.Ordinal);
    }

    private static void ProcessAssemblyTypes(Assembly assembly, MiraPluginInfo info, BasePlugin basePlugin, IMiraPlugin plugin, List<Type> roles, bool isMira)
    {
        foreach (var type in assembly.GetTypes())
        {
            if ((isMira && type.IsAbstract) || type.IsDefined<MiraIgnoreAttribute>())
                continue;

            RegisterEvents(type);

            if (RegisterModifier(type, info))
                continue;

            if (RegisterOptions(type, info))
                continue;

            if (RegisterLocalTabs(type, basePlugin))
                continue;

            if (RegisterRole(type, info, roles))
                continue;

            if (RegisterButton(type, info))
                continue;

            if (RegisterGameOver(type))
                continue;

            if (RegisterMeetingAbility(type, info))
                continue;

            if (RegisterTargetedMeetingAbility(type, info))
                continue;

            if (RegisterGameModeAttribute(type, info))
                continue;

            if (!RegisterColorClasses(type))
                RegisterKeybinds(type, plugin);
        }
    }

    private static void RegisterEvents(Type type)
    {
        foreach (var method in AccessTools.GetDeclaredMethods(type))
        {
            if (!method.TryGetCustomAttribute<RegisterEventAttribute>(out var eventAttribute))
                continue;

            if (!method.IsStatic)
            {
                Error($"Event method {method.Name} in {type.Name} must be static.");
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length != 1 || !parameters[0].ParameterType.IsSubclassOf(typeof(MiraEvent)))
            {
                Error($"Invalid event registration method {method.Name} in {type.Name}");
                continue;
            }

            var paramType = parameters[0].ParameterType;
            MiraEventManager.RegisterEventHandler(paramType, method, eventAttribute.Priority);
        }
    }

    /// <summary>
    /// Get a mira plugin by its GUID.
    /// </summary>
    /// <param name="pluginId">The plugin GUID.</param>
    /// <returns>A <see cref="MiraPluginInfo"/>.</returns>
    public static MiraPluginInfo? GetPluginByGuid(string pluginId)
    {
        return Instance._registeredPlugins.Values.FirstOrDefault(plugin => plugin.PluginId == pluginId);
    }

    private static bool RegisterGameOver(Type type)
    {
        try
        {
            return GameOverManager.RegisterGameOver(type);
        }
        catch (Exception e)
        {
            Error($"Failed to register game over {type.Name}: {e}");
            return false;
        }
    }

    private static bool RegisterOptions(Type type, MiraPluginInfo pluginInfo)
    {
        try
        {
            if (!type.IsAssignableTo(typeof(AbstractOptionGroup)))
            {
                return false;
            }

            if (!ModdedOptionsManager.RegisterGroup(type, pluginInfo))
            {
                return false;
            }

            foreach (var property in type.GetProperties(DeclaredInstanceMembers))
            {
                if (property.IsDefined<MiraIgnoreAttribute>())
                    continue;

                if (property.PropertyType.IsAssignableTo(typeof(IModdedOption)))
                {
                    ModdedOptionsManager.RegisterPropertyOption(type, property, pluginInfo);
                    continue;
                }

                if (property.PropertyType.IsAssignableTo(typeof(IModdedOptionList)))
                {
                    ModdedOptionsManager.RegisterPropertyOptionList(type, property, pluginInfo);
                    continue;
                }

                var attribute = property.GetCustomAttribute<ModdedOptionAttribute>();
                if (attribute != null)
                {
                    ModdedOptionsManager.RegisterAttributeOption(type, attribute, property, pluginInfo);
                    continue;
                }

                var listAttr = property.GetCustomAttribute<ModdedOptionListAttribute>();
                if (listAttr != null)
                {
                    ModdedOptionsManager.RegisterAttributeOptionList(type, listAttr, property, pluginInfo);
                }
            }

            foreach (var field in type.GetFields(DeclaredInstanceMembers)
                .Where(f => !IsAutoPropertyBackingField(f) && (f.FieldType.IsAssignableTo(typeof(IModdedOption)) || f.FieldType.IsAssignableTo(typeof(IModdedOptionList)))))
            {
                Error($"{field.Name} is a field, not a property. Use properties for options.");
            }

            return true;
        }
        catch (Exception e)
        {
            Error($"Failed to register options for {type.Name}: {e}");
        }

        return false;
    }

    private static bool RegisterRole(Type type, MiraPluginInfo pluginInfo, List<Type> roles)
    {
        try
        {
            if (!(typeof(RoleBehaviour).IsAssignableFrom(type) && typeof(ICustomRole).IsAssignableFrom(type)))
            {
                return false;
            }

            if (!ModList.GetById(pluginInfo.PluginId).IsRequiredOnAllClients)
            {
                Error("Custom roles are only supported on all clients.");
                return false;
            }

            roles.Add(type);
            return true;
        }
        catch (Exception e)
        {
            Error($"Failed to register role for {type.Name}: {e}");
        }

        return false;
    }

    private static bool RegisterColorClasses(Type type)
    {
        try
        {
            if (type.GetCustomAttribute<RegisterCustomColorsAttribute>() == null)
            {
                return false;
            }

            if (!type.IsStatic())
            {
                Error($"Color class {type.Name} must be static.");
                return false;
            }

            foreach (var property in type.GetProperties(DeclaredStaticMembers))
            {
                if (!property.PropertyType.IsAssignableTo(typeof(CustomColor)))
                {
                    continue;
                }

                if (property.GetValue(null) is not CustomColor color)
                {
                    Error($"Color property {property.Name} in {type.Name} is not a CustomColor.");
                    continue;
                }

                PaletteManager.CustomColors.Add(color);

                if (color is VariableColor)
                    PaletteManager.VariableColorNames.Add(color.Name);
            }

            foreach (var field in type.GetFields(DeclaredStaticMembers).Where(f => !IsAutoPropertyBackingField(f) && f.FieldType.IsAssignableTo(typeof(CustomColor))))
            {
                Error($"{field.Name} is a field, not a property. Use properties for colors.");
            }

            return true;
        }
        catch (Exception e)
        {
            Error($"Failed to register color class {type.Name}: {e}");
        }

        return false;
    }

    private static bool RegisterModifier(Type type, MiraPluginInfo info)
    {
        try
        {
            return ModifierManager.RegisterModifier(type, info);
        }
        catch (Exception e)
        {
            Error($"Failed to register modifier {type.Name}: {e}");
            return false;
        }
    }

    private static bool RegisterButton(Type type, MiraPluginInfo pluginInfo)
    {
        try
        {
            return CustomButtonManager.RegisterButton(type, pluginInfo);
        }
        catch (Exception e)
        {
            Error($"Failed to register button {type.Name}: {e}");
        }

        return false;
    }

    private static bool RegisterGameModeAttribute(Type type, MiraPluginInfo pluginInfo)
    {
        try
        {
            if (typeof(AbstractGameMode).IsAssignableFrom(type))
                return CustomGameModeManager.RegisterGameMode(type, pluginInfo);
        }
        catch (Exception e)
        {
            Error($"Failed to register gamemode {type.Name}: {e}");
        }

        return false;
    }

    private static bool RegisterLocalTabs(Type type, BasePlugin pluginInfo)
    {
        try
        {
            if (!type.IsAssignableTo(typeof(LocalSettingsTab)))
            {
                return false;
            }

            if (!LocalSettingsManager.RegisterTab(type, pluginInfo))
            {
                return false;
            }

            if (!LocalSettingsManager.TypeToTab.TryGetValue(type, out var tabInstance))
            {
                return false;
            }

            foreach (var property in type.GetProperties(DeclaredInstanceMembers))
            {
                if (property.IsDefined<LocalSettingsButtonAttribute>() &&
                    property.PropertyType.IsAssignableTo(typeof(LocalSettingsButton)))
                {
                    var button = property.GetValue(tabInstance) as LocalSettingsButton;
                    button!.Tab = tabInstance;
                    tabInstance.Buttons.Add(button);
                    continue;
                }

                if (!typeof(ConfigEntryBase).IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                if (property.GetValue(tabInstance) is not ConfigEntryBase configEntry)
                {
                    Error($"Option property {property.Name} in {type.Name} has to be a config entry.");
                    continue;
                }

                if (property.TryGetCustomAttribute<LocalSettingAttribute>(out var attribute))
                {
                    attribute.CreateSetting(type, configEntry);
                }
            }

            foreach (var field in type.GetFields(DeclaredInstanceMembers).Where(f => !IsAutoPropertyBackingField(f) && f.FieldType.IsAssignableTo(typeof(ConfigEntryBase)) && f.IsDefined<LocalSettingAttribute>()))
            {
                Error($"{field.Name} is a field, not a property. Use properties for local settings.");
            }

            return true;
        }
        catch (Exception e)
        {
            Error($"Failed to register options for {type.Name}: {e}");
        }

        return false;
    }

    private static void RegisterKeybinds(Type type, IMiraPlugin plugin)
    {
        try
        {
            if (!type.IsDefined<RegisterCustomKeybindsAttribute>())
            {
                return;
            }

            if (!type.IsStatic())
            {
                Error($"Keybinds class {type.Name} must be static.");
                return;
            }

            foreach (var property in type.GetProperties(DeclaredStaticMembers))
            {
                if (property.PropertyType != typeof(MiraKeybind))
                {
                    continue;
                }

                if (property.GetValue(null) is not MiraKeybind keybind)
                {
                    Error($"Keybind property {property.Name} in {type.Name} is not a MiraKeybind.");
                    continue;
                }

                KeybindManager.Keybinds.Add(keybind);
                keybind.SourcePluginName = plugin.OptionsTitleText.Translate();
            }

            foreach (var field in type.GetFields(DeclaredStaticMembers).Where(f => !IsAutoPropertyBackingField(f) && f.FieldType.IsAssignableTo(typeof(MiraKeybind))))
            {
                Error($"{field.Name} is a field, not a property. Use properties for keybinds.");
            }
        }
        catch (Exception e)
        {
            Error($"Failed to register keybind class {type.Name}: {e}");
        }
    }

    private static bool RegisterMeetingAbility(Type type, MiraPluginInfo info)
    {
        try
        {
            return MeetingButtonManager.RegisterMeetingButton(type, info);
        }
        catch (Exception e)
        {
            Error($"Failed to register meeting ability {type.Name}: {e}");
            return false;
        }
    }

    private static bool RegisterTargetedMeetingAbility(Type type, MiraPluginInfo info)
    {
        try
        {
            return MeetingButtonManager.RegisterTargetedMeetingButton(type, info);
        }
        catch (Exception e)
        {
            Error($"Failed to register targeted meeting ability {type.Name}: {e}");
            return false;
        }
    }
}

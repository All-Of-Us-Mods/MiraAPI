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
using MiraAPI.Colors;
using MiraAPI.Events;
using MiraAPI.GameEnd;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.LocalSettings;
using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using MiraAPI.Presets;
using MiraAPI.Roles;
using MiraAPI.Utilities;

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

    internal const BindingFlags DeclaredInstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    internal const BindingFlags DeclaredStaticMembers = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private readonly Dictionary<string, MiraPluginInfo> _registeredPlugins = [];

    internal MiraPluginInfo[] RegisteredPlugins { get; private set; } = [];
    internal MiraPluginInfo[] RegisteredPluginsWithOptions { get; private set; } = [];
    internal MiraPluginInfo[] PluginsWithOptionsOrGameModes { get; private set; } = [];

    internal Dictionary<MiraPluginInfo, List<Type>> QueuedRoleRegistrations { get; } = [];
    internal static MiraPluginManager Instance { get; } = new();
    internal static ConfigFile MiraApiConfig { get; private set; } = null!;
    internal static MiraPluginInfo MiraApiPluginInfo { get; private set; } = null!;

    /// <summary>
    /// Get a mira plugin by its GUID.
    /// </summary>
    /// <param name="pluginId">The plugin GUID.</param>
    /// <returns>A <see cref="MiraPluginInfo"/>.</returns>
    public static MiraPluginInfo? GetPluginByGuid(string pluginId)
    {
        return Instance._registeredPlugins.GetValueOrDefault(pluginId);
    }

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

            ModifierManager.AllModifiers = new ReadOnlyCollection<IBaseModifier>(ModifierManager.InternalModifiers);
        };
    }

    private MiraPluginInfo RegisterPlugin(Assembly assembly, IMiraPlugin plugin, PluginInfo pluginInfo, BasePlugin basePlugin, bool isMira)
    {
        var info = new MiraPluginInfo(plugin, pluginInfo, assembly);
        var roles = new List<Type>();

        var oldConfigSetting = info.PluginConfig.SaveOnConfigSet;
        info.PluginConfig.SaveOnConfigSet = false;

        ProcessAssemblyTypes(assembly, info, basePlugin, plugin, roles, isMira);

        info.PluginConfig.Save();
        info.PluginConfig.SaveOnConfigSet = oldConfigSetting;

        info.InternalOptionGroups.Sort((x, y) => x.GroupPriority.CompareTo(y.GroupPriority));
        QueuedRoleRegistrations.Add(info, roles);
        _registeredPlugins.Add(pluginInfo.Metadata.GUID, info);

        info.SavePublicCollections();
        PresetManager.CreateDefaultPreset(info);
        PresetManager.LoadPresets(info);

        return info;
    }

    internal static bool IsAutoPropertyBackingField(FieldInfo field)
    {
        return field.IsDefined<CompilerGeneratedAttribute>(false) &&
               field.Name.Contains(">k__BackingField", StringComparison.Ordinal);
    }

    [SuppressMessage("Minor Code Smell", "S3626:Jump statements should not be redundant", Justification = "Code consistency.")]
    private static void ProcessAssemblyTypes(Assembly assembly, MiraPluginInfo info, BasePlugin basePlugin, IMiraPlugin plugin, List<Type> roles, bool isMira)
    {
        if (plugin.ForceRegisterEvents)
        {
            Warning("For optimisation reasons, MiraAPI no longer automatically searches every type for events. Please use RegisterEventsAttribute moving forward.");
        }

        foreach (var type in assembly.GetTypes())
        {
            if ((isMira && type.IsAbstract) || type.IsDefined<MiraIgnoreAttribute>())
                continue;

            RegisterEvents(type, plugin);

            if (RegisterOptions(type, info))
                continue;

            if (RegisterLocalTabs(type, basePlugin))
                continue;

            if (RegisterColors(type, info))
                continue;

            if (RegisterRole(type, info, roles))
                continue;

            if (RegisterModifier(type, info))
                continue;

            if (RegisterButton(type, info))
                continue;

            if (RegisterKeybinds(type, plugin))
                continue;

            if (RegisterGameModeAttribute(type, info))
                continue;

            if (RegisterGameOver(type))
                continue;

            if (RegisterMeetingAbility(type, info))
                continue;

            if (RegisterTargetedMeetingAbility(type, info))
                continue;
        }
    }

    private static void RegisterEvents(Type type, IMiraPlugin miraPlugin)
    {
        try
        {
            MiraEventManager.RegisterEvents(type, miraPlugin);
        }
        catch (Exception e)
        {
            Error($"Failed to register events in {type.Name}: {e}");
        }
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
            return ModdedOptionsManager.RegisterGroup(type, pluginInfo);
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
            return CustomRoleManager.RegisterRole(type, pluginInfo, roles);
        }
        catch (Exception e)
        {
            Error($"Failed to register role for {type.Name}: {e}");
        }

        return false;
    }

    private static bool RegisterColors(Type type, MiraPluginInfo miraPlugin)
    {
        try
        {
            return PaletteManager.RegisterColors(type, miraPlugin);
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
        }

        return false;
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
            return LocalSettingsManager.RegisterLocalTabs(type, pluginInfo);
        }
        catch (Exception e)
        {
            Error($"Failed to register options for {type.Name}: {e}");
        }

        return false;
    }

    private static bool RegisterKeybinds(Type type, IMiraPlugin plugin)
    {
        try
        {
            return KeybindManager.RegisterKeybinds(type, plugin);
        }
        catch (Exception e)
        {
            Error($"Failed to register keybind class {type.Name}: {e}");
        }

        return false;
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
        }

        return false;
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
        }

        return false;
    }
}

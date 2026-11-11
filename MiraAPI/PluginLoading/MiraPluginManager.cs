using System;
using System.Collections.Generic;
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
    internal delegate bool TypeRegistrationHandler(in RegistrationContext context);

    internal readonly record struct RegistrationContext(Type Type, MiraPluginInfo PluginInfo, IMiraPlugin MiraPlugin, BasePlugin BasePlugin, List<Type> Roles);

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

    private static readonly (string LogContext, TypeRegistrationHandler Handler)[] RegistrationHandlers =
    [
        ("events in", MiraEventManager.RegisterEvents),
        ("options for", ModdedOptionsManager.RegisterGroup),
        ("options for", LocalSettingsManager.RegisterLocalTabs),
        ("color class", PaletteManager.RegisterColors),
        ("role for", CustomRoleManager.RegisterRole),
        ("modifier", ModifierManager.RegisterModifier),
        ("button", CustomButtonManager.RegisterButton),
        ("keybind class", KeybindManager.RegisterKeybinds),
        ("gamemode", CustomGameModeManager.RegisterGameMode),
        ("game over", GameOverManager.RegisterGameOver),
        ("meeting ability", MeetingButtonManager.RegisterMeetingButton),
        ("targeted meeting ability", MeetingButtonManager.RegisterTargetedMeetingButton),
    ];

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
            CustomButtonManager.RegisterAllButtons();
            ModifierManager.RegisterAllModifiers();

            // Cache all the registered plugins into an array for easy access
            RegisteredPlugins = [.. _registeredPlugins.Values];
            RegisteredPluginsWithOptions = [.. RegisteredPlugins.Where(m => m.MiraPlugin.DisplayOnOptionsMenu)];
            PluginsWithOptionsOrGameModes = [.. RegisteredPlugins.Where(m => m.MiraPlugin.DisplayOnOptionsMenu || m.GameModes.Count > 0)];
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

    private static void ProcessAssemblyTypes(Assembly assembly, MiraPluginInfo info, BasePlugin basePlugin, IMiraPlugin plugin, List<Type> roles, bool isMira)
    {
        if (plugin.ForceRegisterEvents)
        {
            Warning("For optimisation reasons, MiraAPI no longer automatically searches every type for events. Please use RegisterEventContainerAttribute moving forward.");
        }

        foreach (var type in assembly.GetTypes())
        {
            if ((isMira && type.IsAbstract) || type.IsDefined<MiraIgnoreAttribute>())
                continue;

            var context = new RegistrationContext(type, info, plugin, basePlugin, roles);

            foreach (var (logContext, handler) in RegistrationHandlers)
            {
                try
                {
                    if (handler(in context))
                        break;
                }
                catch (Exception e)
                {
                    Error($"Failed to register {logContext} {type.Name}: {e}");
                }
            }
        }
    }
}

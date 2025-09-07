using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using MiraAPI.Colors;
using MiraAPI.Events;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Presets;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking;
using Reactor.Utilities;

namespace MiraAPI.PluginLoading;

/// <summary>
/// Mira Plugin manager.
/// </summary>
public sealed class MiraPluginManager
{
    private readonly Dictionary<Assembly, MiraPluginInfo> _registeredPlugins = [];

    internal MiraPluginInfo[] RegisteredPlugins { get; private set; } = null!;

    internal Dictionary<MiraPluginInfo, List<Type>> QueuedRoleRegistrations { get; } = [];
    internal static MiraPluginManager Instance { get; private set; } = new();

    internal void Initialize()
    {
        Instance = this;
        IL2CPPChainloader.Instance.PluginLoad += (pluginInfo, assembly, plugin) =>
        {
            if (plugin is not IMiraPlugin miraPlugin)
            {
                return;
            }

            var info = new MiraPluginInfo(miraPlugin, pluginInfo);
            var roles = new List<Type>();

            var oldConfigSetting = info.PluginConfig.SaveOnConfigSet;
            info.PluginConfig.SaveOnConfigSet = false;

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<MiraIgnoreAttribute>() != null)
                {
                    continue;
                }

                foreach (var method in type.GetMethods())
                {
                    var eventAttribute = method.GetCustomAttribute<RegisterEventAttribute>();
                    if (eventAttribute == null)
                    {
                        continue;
                    }

                    if (!method.IsStatic)
                    {
                        Logger<MiraApiPlugin>.Error($"Event method {method.Name} in {type.Name} must be static.");
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length != 1 || !parameters[0].ParameterType.IsSubclassOf(typeof(MiraEvent)))
                    {
                        Logger<MiraApiPlugin>.Error($"Invalid event registration method {method.Name} in {type.Name}");
                        continue;
                    }

                    var paramType = parameters[0].ParameterType;
                    MiraEventManager.RegisterEventHandler(paramType, method, eventAttribute.Priority);
                }

                if (RegisterModifier(type, info))
                {
                    continue;
                }

                if (RegisterOptions(type, info))
                {
                    continue;
                }

                if (RegisterRole(type, info, out var role))
                {
                    roles.Add(role);
                    continue;
                }

                if (RegisterButton(type, info))
                {
                    continue;
                }

                if (RegisterGameOver(type))
                {
                    continue;
                }

                RegisterColorClasses(type);
                RegisterKeybinds(type, plugin);
            }

            info.PluginConfig.Save();
            info.PluginConfig.SaveOnConfigSet = oldConfigSetting;

            info.InternalOptionGroups.Sort((x, y) => x.GroupPriority.CompareTo(y.GroupPriority));
            QueuedRoleRegistrations.Add(info, roles);

            _registeredPlugins.Add(assembly, info);

            info.SavePublicCollections();
            PresetManager.CreateDefaultPreset(info);
            PresetManager.LoadPresets(info);
            Logger<MiraApiPlugin>.Info($"Registering mod {pluginInfo.Metadata.GUID} with Mira API.");
        };
        IL2CPPChainloader.Instance.Finished += PaletteManager.RegisterAllColors;
        IL2CPPChainloader.Instance.Finished += () =>
        {
            // Save all buttons into a read-only collection for easy access
            CustomButtonManager.Buttons = new ReadOnlyCollection<CustomActionButton>(CustomButtonManager.CustomButtons);

            // Cache all the registered plugins into an array for easy access
            RegisteredPlugins = [.. _registeredPlugins.Values];

            ModifierManager.Modifiers = new ReadOnlyCollection<BaseModifier>(ModifierManager.InternalModifiers);
        };

        RegisterKeybinds(typeof(MiraGlobalKeybinds), PluginSingleton<MiraApiPlugin>.Instance);
    }

    /// <summary>
    /// Get a mira plugin by its GUID.
    /// </summary>
    /// <param name="pluginId">The plugin GUID.</param>
    /// <returns>A MiraPluginInfo.</returns>
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
            Logger<MiraApiPlugin>.Error($"Failed to register game over {type.Name}: {e}");
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

            foreach (var property in type.GetProperties())
            {
                if (property.GetMethod?.IsStatic == true)
                {
                    Logger<MiraApiPlugin>.Error($"Option property {property.Name} in {type.Name} must not be static.");
                    continue;
                }

                if (property.PropertyType.IsAssignableTo(typeof(IModdedOption)))
                {
                    ModdedOptionsManager.RegisterPropertyOption(type, property, pluginInfo);
                    continue;
                }

                var attribute = property.GetCustomAttribute<ModdedOptionAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                ModdedOptionsManager.RegisterAttributeOption(type, attribute, property, pluginInfo);
            }

            foreach (var field in type.GetFields().Where(f => f.FieldType.IsAssignableTo(typeof(IModdedOption))))
            {
                Logger<MiraApiPlugin>.Error($"{field.Name} is a field, not a property. Use properties for options.");
            }

            return true;
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Error($"Failed to register options for {type.Name}: {e.ToString()}");
        }
        return false;
    }

    private static bool RegisterRole(Type type, MiraPluginInfo pluginInfo, [NotNullWhen(true)] out Type? role)
    {
        role = null;
        try
        {
            if (!(typeof(RoleBehaviour).IsAssignableFrom(type) && typeof(ICustomRole).IsAssignableFrom(type)))
            {
                return false;
            }

            if (!ModList.GetById(pluginInfo.PluginId).IsRequiredOnAllClients)
            {
                Logger<MiraApiPlugin>.Error("Custom roles are only supported on all clients.");
                return false;
            }

            role = type;
            return true;
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Error($"Failed to register role for {type.Name}: {e}");
        }
        return false;
    }

    private static void RegisterColorClasses(Type type)
    {
        try
        {
            if (type.GetCustomAttribute<RegisterCustomColorsAttribute>() == null)
            {
                return;
            }

            if (!type.IsStatic())
            {
                Logger<MiraApiPlugin>.Error($"Color class {type.Name} must be static.");
                return;
            }

            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType != typeof(CustomColor))
                {
                    continue;
                }

                if (property.GetValue(null) is not CustomColor color)
                {
                    Logger<MiraApiPlugin>.Error($"Color property {property.Name} in {type.Name} is not a CustomColor.");
                    continue;
                }

                PaletteManager.CustomColors.Add(color);
            }

            foreach (var field in type.GetFields().Where(f => f.FieldType.IsAssignableTo(typeof(CustomColor))))
            {
                Logger<MiraApiPlugin>.Error($"{field.Name} is a field, not a property. Use properties for colors.");
            }
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Error($"Failed to register color class {type.Name}: {e}");
        }
    }

    private static bool RegisterModifier(Type type, MiraPluginInfo info)
    {
        try
        {
            return ModifierManager.RegisterModifier(type, info);
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Error($"Failed to register modifier {type.Name}: {e}");
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
            Logger<MiraApiPlugin>.Error($"Failed to register button {type.Name}: {e}");
        }

        return false;
    }

    private static void RegisterKeybinds(Type type, BasePlugin source)
    {
        try
        {
            if (type.GetCustomAttribute<RegisterCustomKeybindsAttribute>() == null)
            {
                return;
            }

            if (!type.IsStatic())
            {
                Logger<MiraApiPlugin>.Error($"Keybinds class {type.Name} must be static.");
                return;
            }

            foreach (var property in type.GetProperties())
            {
                if (property.PropertyType != typeof(MiraKeybind))
                {
                    continue;
                }

                if (property.GetValue(null) is not MiraKeybind keybind)
                {
                    Logger<MiraApiPlugin>.Error($"Keybind property {property.Name} in {type.Name} is not a MiraKeybind.");
                    continue;
                }

                KeybindManager.Keybinds.Add(keybind);
                if (source is IMiraPlugin miraPlugin)
                {
                    keybind.SourcePluginName = miraPlugin.OptionsTitleText;
                }
                else if (source is MiraApiPlugin)
                {
                    keybind.SourcePluginName = "MiraAPI";
                }
            }

            foreach (var field in type.GetFields().Where(f => f.FieldType.IsAssignableTo(typeof(MiraKeybind))))
            {
                Logger<MiraApiPlugin>.Error($"{field.Name} is a field, not a property. Use properties for keybinds.");
            }
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Error($"Failed to register keybind class {type.Name}: {e}");
        }
    }
}

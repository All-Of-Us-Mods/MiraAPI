using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Presets;

/// <summary>
/// Provides functionality to manage and load game <see cref="OptionPreset"/>s for plugins.
/// </summary>
public static class PresetManager
{
    /// <summary>
    /// Gets the directory where the presets are stored.
    /// </summary>
    public static string PresetDirectory { get; } = Path.GetFullPath("mira_presets", Application.persistentDataPath);

    internal static Dictionary<MiraPluginInfo, OptionPreset> DefaultPresets { get; } = [];
    internal static List<OptionPreset> InternalMasterPresets { get; } = [];

    /// <summary>
    /// Gets a <see cref="IReadOnlyCollection{T}"/> of this plugin's <see cref="OptionPreset"/>s.
    /// </summary>
    public static IReadOnlyCollection<OptionPreset> MasterPresets { get; internal set; } = null!;

    internal static void CreateDefaultPreset(MiraPluginInfo plugin)
    {
        var presetPath = Path.Combine(PresetDirectory, plugin.PluginId);
        if (!Directory.Exists(presetPath))
        {
            Directory.CreateDirectory(presetPath);
        }

        var presetConfig = new ConfigFile(Path.Combine(presetPath, "Default.cfg"), false);

        foreach (var option in plugin.InternalOptions.Where(x => x.IncludeInPreset))
        {
            option.SaveToPreset(presetConfig, true);
        }

        foreach (var role in plugin.InternalRoles.Values.OfType<ICustomRole>())
        {
            role.SaveToPreset(presetConfig);
        }

        presetConfig.Save();
        DefaultPresets.Add(plugin, new OptionPreset("Default", plugin, presetConfig));
    }

    /// <summary>
    /// Loads the <see cref="OptionPreset"/>s for the specified plugin by reading the <see cref="ConfigFile"/>s from the preset directory.
    /// </summary>
    /// <param name="plugin">The plugin for which the <see cref="OptionPreset"/>s should be loaded.</param>
    [SuppressMessage("Style", "IDE0031:Use null propagation", Justification = "Using null propagation bypasses Unity's lifetime checks.")]
    public static void LoadPresets(MiraPluginInfo plugin)
    {
        foreach (var btn in plugin.InternalPresets.Select(x => x.PresetButton))
        {
            if (btn != null)
            {
                btn.Destroy();
            }
        }

        plugin.InternalPresets.Clear();
        if (!Directory.Exists(PresetDirectory))
        {
            Directory.CreateDirectory(PresetDirectory);
        }

        var pluginPresetPath = Path.Combine(PresetDirectory, plugin.PluginId);
        if (!Directory.Exists(pluginPresetPath))
        {
            Directory.CreateDirectory(pluginPresetPath);
        }

        foreach (var file in Directory.GetFiles(pluginPresetPath, "*.cfg"))
        {
            var fileName = Path.GetFileName(file);
            Info($"Loading preset file {fileName}");
            var presetName = Path.GetFileNameWithoutExtension(file);
            var presetConfig = new ConfigFile(file, false)
            {
                SaveOnConfigSet = false,
            };

            foreach (var option in plugin.InternalOptions.Where(x => x.IncludeInPreset))
            {
                option.Bind(presetConfig);
            }

            foreach (var role in plugin.InternalRoles.Values.OfType<ICustomRole>().Where(x => !x.Configuration.HideSettings))
            {
                role.BindConfig(presetConfig);
            }

            presetConfig.Save();

            plugin.InternalPresets.Add(new OptionPreset(presetName, plugin, presetConfig));
        }
        plugin.Presets = [.. plugin.InternalPresets];
    }

    /// <summary>
    /// Loads the master presets by reading the <see cref="ConfigFile"/>s from the preset directory.
    /// </summary>
    public static void LoadMasterPreset()
    {
#pragma warning disable S125 // Sections of code should not be commented out
        /*foreach (var btn in plugin.InternalPresets.Select(x => x.PresetButton))
                {
                    if (btn != null)
                    {
                        Object.DestroyImmediate(btn);
                    }
                }

                plugin.InternalPresets.Clear();*/
#pragma warning restore S125 // Sections of code should not be commented out
        if (!Directory.Exists(PresetDirectory))
        {
            Directory.CreateDirectory(PresetDirectory);
        }

        var pluginPresetPath = Path.Combine(PresetDirectory, "MasterPresets");
        if (!Directory.Exists(pluginPresetPath))
        {
            Directory.CreateDirectory(pluginPresetPath);
        }

        foreach (var file in Directory.GetFiles(pluginPresetPath, "*.cfg"))
        {
            var fileName = Path.GetFileName(file);
            Info($"Loading preset file {fileName}");

#pragma warning disable S125 // Sections of code should not be commented out
            // var presetName = Path.GetFileNameWithoutExtension(file);
#pragma warning restore S125 // Sections of code should not be commented out
            var presetConfig = new ConfigFile(file, false)
            {
                SaveOnConfigSet = false,
            };
            foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
            {
                foreach (var option in plugin.InternalOptions.Where(x => x.IncludeInPreset))
                {
                    option.Bind(presetConfig);
                }

                foreach (var role in plugin.InternalRoles.Values.OfType<ICustomRole>().Where(x => !x.Configuration.HideSettings))
                {
                    role.BindConfig(presetConfig);
                }
            }

            presetConfig.Save();

#pragma warning disable S125 // Sections of code should not be commented out
            // InternalMasterPresets.Add(new OptionPreset(presetName, plugin, presetConfig));
#pragma warning restore S125 // Sections of code should not be commented out
        }
        MasterPresets = [.. InternalMasterPresets];
    }
}

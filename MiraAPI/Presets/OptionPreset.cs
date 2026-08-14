using System.Linq;
using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Presets;

/// <summary>
/// Represents a preset of game options that can be applied to the game.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OptionPreset"/> class with the specified name and configuration file.
/// </remarks>
/// <param name="name">The name of the preset.</param>
/// <param name="plugin">The plugin associated with the preset.</param>
/// <param name="presetConfig">The configuration file for the preset.</param>
public class OptionPreset(string name, MiraPluginInfo plugin, ConfigFile presetConfig)
{
    /// <summary>
    /// Gets the name of the preset.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the plugin associated with the preset.
    /// </summary>
    public MiraPluginInfo Plugin { get; } = plugin;

    /// <summary>
    /// Gets the <see cref="ConfigFile"/> of the plugin associated with the preset.
    /// </summary>
    public ConfigFile PluginConfig { get; } = plugin.PluginConfig;

    /// <summary>
    /// Gets the <see cref="ConfigFile"/> for the preset.
    /// </summary>
    public ConfigFile PresetConfig { get; } = presetConfig;

    /// <summary>
    /// Gets or sets the button associated with the preset in the UI.
    /// </summary>
    public GameObject? PresetButton { get; set; }

    /// <summary>
    /// Loads the preset by applying the values from the preset configuration to the plugin configuration.
    /// </summary>
    public void LoadPreset()
    {
        PresetConfig.Reload();
        foreach (var option in Plugin.InternalOptions)
        {
            option.LoadFromPreset(PresetConfig);
        }

        ModdedOptionsManager.SyncAllOptions();

        foreach (var role in Plugin.InternalRoles.Values.OfType<ICustomRole>())
        {
            role.LoadFromPreset(PresetConfig);
        }

        CustomRoleManager.SyncAllRoleSettings();
    }

    /// <summary>
    /// Resets the specified option with the preset provided.
    /// </summary>
    /// <param name="baseOption">The option to reset.</param>
    public void ResetOption(OptionBehaviour baseOption)
    {
        var selectedOpt = Plugin.InternalOptions.First(x => x.OptionBehaviour == baseOption);
        selectedOpt.LoadFromPreset(PresetConfig);
#pragma warning disable S125 // Sections of code should not be commented out
        /*ModdedOptionsManager.SyncAllOptions();

        CustomRoleManager.SyncAllRoleSettings();*/
#pragma warning restore S125 // Sections of code should not be commented out
    }

    /// <summary>
    /// Gets whether the specified option is included in the preset.
    /// </summary>
    /// <param name="baseOption">The option to check.</param>
    /// <returns>The value of whether the option is in the preset.</returns>
    public bool IsOptionInPreset(OptionBehaviour baseOption)
    {
        var selectedOpt = Plugin.InternalOptions.First(x => x.OptionBehaviour == baseOption);
        return selectedOpt.ConfigDefinition != null && PresetConfig.ContainsKey(selectedOpt.ConfigDefinition);
    }
}

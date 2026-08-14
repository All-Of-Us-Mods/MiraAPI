using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.Patches;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI;

/// <summary>
/// Mira API <see cref="ConfigFile"/> Handler.
/// </summary>
public class MiraApiSettings(ConfigFile config) : LocalSettingsTab(config)
{
    /// <inheritdoc />
    public override string TabName => "Mira API";

    /// <inheritdoc />
    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabButtonHoverColor = MiraApiPlugin.MiraColor,
        TabIcon = MiraAssets.SettingsIcon,
        HideIconOnHover = false,
    };

    /// <summary>
    /// Gets or sets the value stored for scaling buttons properly.
    /// </summary>
    public static float OldButtonScaleFactor { get; set; }

    /// <inheritdoc />
    public override void Open()
    {
        base.Open();
        OldButtonScaleFactor = ButtonUIFactorSlider.Value;
    }

    /// <inheritdoc />
    public override void OnOptionChanged(ConfigEntryBase configEntry)
    {
        base.OnOptionChanged(configEntry);
        if (configEntry == ButtonUIFactorSlider)
        {
            if (HudManager.InstanceExists)
            {
                HudManagerPatches.ResizeUI(1f / OldButtonScaleFactor);
                HudManagerPatches.ResizeUI(ButtonUIFactorSlider.Value);
            }
            OldButtonScaleFactor = ButtonUIFactorSlider.Value;
        }
        else if (configEntry == SetFpsSlider)
        {
            Application.targetFrameRate = (int)SetFpsSlider.Value;
        }
    }

    /// <summary>
    /// Gets whether the modifiers hud should be on the left side of the screen (under roles/task tab). Recommended for streamers.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> ModifiersHudLeftSide { get; private set; } = config.Bind("Visuals/UI", "Show Modifiers HUD on Left Side", false);

    /// <summary>
    /// Gets whether to show keybinds on buttons.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> ShowKeybinds { get; private set; } = config.Bind("Visuals/UI", "Show Keybinds on Buttons", true);

    /// <summary>
    /// Gets the scale of the buttons.
    /// </summary>
    [LocalSliderSetting(min: 0.5f, max: 1.5f, suffixType: MiraNumberSuffixes.Multiplier, formatString: "0.00", displayValue: true)]
    public ConfigEntry<float> ButtonUIFactorSlider { get; private set; } = config.Bind("Visuals/UI", "Button Scale Factor", 0.75f);

    /// <summary>
    /// Gets the fps specified by the player.
    /// </summary>
    [LocalSliderSetting(min: 60f, max: 240f, suffixType: MiraNumberSuffixes.None, formatString: "0", displayValue: true, roundValue: true)]
    public ConfigEntry<float> SetFpsSlider { get; private set; } = config.Bind("Visuals/UI", "Max FPS", 120f);

    /// <summary>
    /// Gets whether to apply cosmetic changes to the TaskAdder.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> PrettyTaskAdder { get; private set; } = config.Bind("Freeplay", "Pretty Task Laptop", true);

    /// <summary>
    /// Gets whether to show the red flash from sabotages.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> EnableSabotageFlashes { get; private set; } = config.Bind("Accessibility", "Enable Sabotage Flashes", true);

    /// <summary>
    /// Gets whether to enable the sabotage sound effects or not.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> EnableSabotageBlares { get; private set; } = config.Bind("Accessibility", "Enable Sabotage Blare", true);
}

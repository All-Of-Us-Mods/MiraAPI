using System.Collections;
using BepInEx.Configuration;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Hud;
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
    public override string TabName => "MiraApi";

    /// <inheritdoc />
    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabButtonHoverColor = MiraApiPlugin.MiraColor,
        TabIcon = MiraAssets.SettingsIcon,
        HideIconOnHover = false,
    };

    /// <summary>
    /// Gets or sets the value stored for scaling ability buttons properly.
    /// </summary>
    public static float OldButtonScaleFactor { get; set; }

    /// <summary>
    /// Gets or sets the value stored for scaling UI buttons properly.
    /// </summary>
    public static float OldUiButtonScaleFactor { get; set; }

    /// <inheritdoc />
    public override void Open()
    {
        base.Open();
        OldButtonScaleFactor = ButtonUIFactorSlider.Value;
        OldUiButtonScaleFactor = TopRightButtonsFactorSlider.Value;
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
        else if (configEntry == TopRightButtonsFactorSlider)
        {
            if (MiraHudHelper.Instance)
            {
                ResizeUI(TopRightButtonsFactorSlider.Value);
            }
            OldUiButtonScaleFactor = TopRightButtonsFactorSlider.Value;
        }
        else if (configEntry == WikiOnBottomRow)
        {
            SetUpButtonPositions();
        }
        else if (configEntry == SetFpsSlider)
        {
            Application.targetFrameRate = (int)SetFpsSlider.Value;
        }
    }

    /// <summary>
    /// Sets up the button positions in the UI.
    /// </summary>
    public static void SetUpButtonPositions()
    {
        var topUi = MiraHudHelper.UiTopRight;
        var extraTopUi = MiraHudHelper.ExtraUiTopRight;

        if (!topUi || !extraTopUi)
            return;

        var genericEvent = new UiButtonResetEvent();
        MiraEventManager.InvokeEvent(genericEvent);

        var genericEvent2 = new UiButtonPostResetEvent(topUi, extraTopUi);
        MiraEventManager.InvokeEvent(genericEvent2);
        MiraHudHelper.UiGrid.ArrangeChilds();
        MiraHudHelper.ExtraUiGrid.ArrangeChilds();
    }

    /// <summary>
    /// Resizes the UI as a coroutine.
    /// </summary>
    /// <returns>The coroutine that resizes the UI.</returns>
    public static IEnumerator CoResizeSettingsUI()
    {
        while (!HudManager.Instance || !MiraHudHelper.UiGrid || !MiraHudHelper.ExtraUiGrid)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.01f);
        ResizeUI(LocalSettingsTabSingleton<MiraApiSettings>.Instance.TopRightButtonsFactorSlider.Value);
    }

    /// <summary>
    /// Resizes the UI based on the provided scaling factor.
    /// </summary>
    /// <param name="scaleFactor">The factor by which the UI needs to be scaled with.</param>
    public static void ResizeUI(float scaleFactor)
    {
        var alteredScale = scaleFactor * 0.85f;
        var actualScaleVal = scaleFactor * 1.176470588235294f;
        var actualScale = new Vector3(actualScaleVal, actualScaleVal, 1);
        var baseAspect = MiraHudHelper.UiAspectPos;
        var baseGrid = MiraHudHelper.UiGrid;
        var baseUi = MiraHudHelper.UiTopRight;
        if (baseUi && baseAspect && baseGrid)
        {
            baseAspect.DistanceFromEdge = new Vector2(0.435f * scaleFactor, 0.475f * scaleFactor);

            foreach (var button in baseUi.GetComponentsInChildren<PassiveButton>(true))
            {
                if (button.gameObject == null)
                {
                    continue;
                }
                if (button.transform.name.Contains("Friends List Button"))
                {
                    button.gameObject.transform.localScale = new Vector3(0.2675f * actualScaleVal, 0.2675f * actualScaleVal, 1);
                    continue;
                }

                button.gameObject.transform.localScale = actualScale;
            }

            baseGrid.CellSize = new Vector2(alteredScale, alteredScale);
            if (baseGrid.gameObject.transform.childCount != 0)
            {
                baseGrid.ArrangeChilds();
            }
            baseAspect.AdjustPosition();
        }

        var extraAspect = MiraHudHelper.ExtraUiAspectPos;
        var extraGrid = MiraHudHelper.ExtraUiGrid;
        var extraUi = MiraHudHelper.ExtraUiTopRight;
        if (!extraUi || !extraAspect || !extraGrid) return;
        extraAspect.DistanceFromEdge = new Vector3(0.435f * scaleFactor, 1.25f * scaleFactor, 0f);

        foreach (var button in extraUi.GetAllChildren())
        {
            if (button.gameObject == null)
            {
                continue;
            }
            if (button.transform.name.Contains("Modifiers"))
            {
                button.gameObject.transform.localScale = new Vector3(0.65f * scaleFactor, 0.65f * scaleFactor, 1);
                continue;
            }

            button.gameObject.transform.localScale = actualScale;
        }

        extraGrid.CellSize = new Vector2(alteredScale, alteredScale);
        if (extraGrid.gameObject.transform.childCount != 0)
        {
            extraGrid.ArrangeChilds();
        }
        extraAspect.AdjustPosition();
    }

    /// <summary>
    /// Gets the scale of the ability buttons.
    /// </summary>
    [LocalSliderSetting(min: 0.5f, max: 1.5f, suffixType: MiraNumberSuffixes.Multiplier, formatString: "0.00", displayValue: true)]
    public ConfigEntry<float> ButtonUIFactorSlider { get; private set; } = config.Bind("MiraApi.VisualsUi", "MiraApi.ButtonScaleFactor", 0.75f);

    /// <summary>
    /// Gets the scale of the UI buttons.
    /// </summary>
    [LocalSliderSetting(min: 0.3f, max: 2f, suffixType: MiraNumberSuffixes.Multiplier, formatString: "0.00", displayValue: true)]
    public ConfigEntry<float> TopRightButtonsFactorSlider { get; private set; } = config.Bind("MiraApi.VisualsUi", "MiraApi.TopRightButtonScaleFactor", 1f);

    /// <summary>
    /// Gets the fps specified by the player.
    /// </summary>
    [LocalSliderSetting(min: 60f, max: 240f, suffixType: MiraNumberSuffixes.None, formatString: "0", displayValue: true, roundValue: true)]
    public ConfigEntry<float> SetFpsSlider { get; private set; } = config.Bind("MiraApi.VisualsUi", "Max FPS", 120f);

    /// <summary>
    /// Gets whether the modifiers hud should be on the left side of the screen (under roles/task tab). Recommended for streamers.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> ModifiersHudLeftSide { get; private set; } = config.Bind("MiraApi.VisualsUi", "MiraApi.ShowModifiersHudOnLeftSide", false);

    /// <summary>
    /// Gets whether to show keybinds on buttons.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> ShowKeybinds { get; private set; } = config.Bind("MiraApi.VisualsUi", "MiraApi.ShowKeybindsOnButtons", true);

    /// <summary>
    /// Gets the flag indicating whether the wiki is on the bottom row.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> WikiOnBottomRow { get; private set; } = config.Bind("MiraApi.VisualsUi", "MiraApi.WikiOnBottomRow", true);

    /// <summary>
    /// Gets whether to show the red flash from sabotages.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> EnableSabotageFlashes { get; private set; } = config.Bind("MiraApi.Accessibility", "MiraApi.EnableSabotageFlashes", true);

    /// <summary>
    /// Gets whether to enable the sabotage sound effects or not.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> EnableSabotageBlares { get; private set; } = config.Bind("MiraApi.Accessibility", "MiraApi.EnableSabotageBlare", true);
}

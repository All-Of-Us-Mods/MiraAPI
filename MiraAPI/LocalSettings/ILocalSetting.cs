using BepInEx.Configuration;
using UnityEngine;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Interface for all local settings.
/// </summary>
public interface ILocalSetting
{
    /// <summary>
    /// Gets the preferred name of a setting.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of a setting.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the setting's <see cref="GameObject"/>.
    /// </summary>
    GameObject? Setting { get; }

    /// <summary>
    /// Gets the setting's config entry.
    /// </summary>
    ConfigEntryBase ConfigEntry { get; }

    /// <summary>
    /// Used to create the setting.
    /// </summary>
    /// <param name="toggle"><see cref="ToggleButtonBehaviour"/> template.</param>
    /// <param name="slider"><see cref="SlideBar"/> template.</param>
    /// <param name="parent">The parent <see cref="Transform"/> of the setting.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="order">The order.</param>
    /// <param name="last">Whether it's the last on the row.</param>
    /// <returns>The created setting's <see cref="GameObject"/>.</returns>
    GameObject? CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent, ref float offset, ref int order, bool last);

    /// <summary>
    /// Used to refresh the setting's information, usually for localization.
    /// </summary>
    void RefreshOption();
}

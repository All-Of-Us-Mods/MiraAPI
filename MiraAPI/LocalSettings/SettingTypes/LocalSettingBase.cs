using System;
using System.Linq;
using BepInEx.Configuration;
using MiraAPI.PluginLoading;
using UnityEngine;

namespace MiraAPI.LocalSettings.SettingTypes;

/// <inheritdoc />
[MiraIgnore]
public abstract class LocalSettingBase<T> : ILocalSetting
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public GameObject? Setting { get; } = null!;

    /// <inheritdoc />
    public ConfigEntryBase ConfigEntry { get; }

    /// <summary>
    /// Gets the tab of the local setting.
    /// </summary>
    public LocalSettingsTab? Tab => LocalSettingsManager.Tabs.FirstOrDefault(x => x.Settings.Contains(this));

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSettingBase{T}"/> class.
    /// </summary>
    /// <param name="tab">The tab to create the setting in.</param>
    /// <param name="configEntry">The config entry.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="description">The description of the setting.</param>
    protected LocalSettingBase(Type tab, ConfigEntryBase configEntry, string? name = null, string? description = null)
    {
        ConfigEntry = configEntry;
        Name = name ?? ConfigEntry.Definition.Key;
        Description = description ?? ConfigEntry.Description.Description;
        LocalSettingsManager.TypeToTab[tab].Settings.Add(this);
    }

    /// <summary>
    /// Creates an instance of the setting.
    /// </summary>
    /// <param name="toggle">Toggle option to create.</param>
    /// <param name="slider">Slider option to create.</param>
    /// <param name="parent">The parent options menu.</param>
    /// <param name="offset">the Y Offset for the option in the menu.</param>
    /// <param name="order">The order of the option.</param>
    /// <param name="last">Whether the option is the last in a row.</param>
    /// <returns>The created setting.</returns>
    public abstract GameObject CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent, ref float offset, ref int order, bool last);

    /// <inheritdoc/>
    public abstract void RefreshOption();

    /// <summary>
    /// Returns the formated string to use in the text of the setting.
    /// </summary>
    /// <returns>The value text.</returns>
    protected virtual string GetValueText()
    {
        return string.Empty;
    }

    /// <summary>
    /// Gets the value of the config entry, cast to <typeparamref name="T"/>.
    /// </summary>
    /// <returns>The <typeparamref name="T"/> value.</returns>
    public virtual T GetValue()
    {
        return (T)ConfigEntry.BoxedValue;
    }

    /// <summary>
    /// Sets the value of the config entry.
    /// </summary>
    /// <param name="value">The value to set to.</param>
    public virtual void SetValue(T value)
    {
        ConfigEntry.BoxedValue = value;
        Tab?.OnOptionChanged(ConfigEntry);
    }
}

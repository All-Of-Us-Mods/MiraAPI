using System;
using BepInEx.Configuration;

namespace MiraAPI.LocalSettings.Attributes;

/// <summary>
/// Base class for all local settings attributes.
/// </summary>
/// <param name="name">The name of the setting. Defaults to entry key.</param>
/// <param name="description">The description of the setting. Defaults to entry description.</param>
[AttributeUsage(AttributeTargets.Property)]
public abstract class LocalSettingAttribute(
#pragma warning disable CS9113 // Parameter is unread (Justification: No idea why they exist, but they probably do for a reason unknown to me.)
    string? name = null,
    string? description = null
#pragma warning restore CS9113 // Parameter is unread
    ) : Attribute
{
    /// <summary>
    /// Returns the created <see cref="ILocalSetting"/> object.
    /// </summary>
    /// <param name="tab">Gets the tab where the setting is located.</param>
    /// <param name="configEntryBase">Gets the config entry the setting is attached to.</param>
    /// <returns>The created <see cref="ILocalSetting"/>.</returns>
    public abstract ILocalSetting CreateSetting(Type tab, ConfigEntryBase configEntryBase);
}

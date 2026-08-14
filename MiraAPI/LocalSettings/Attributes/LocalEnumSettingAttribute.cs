using System;
using BepInEx.Configuration;
using MiraAPI.LocalSettings.SettingTypes;

namespace MiraAPI.LocalSettings.Attributes;

/// <summary>
/// Creates a <see cref="LocalEnumSetting"/> for the <see cref="ConfigEntry{T}"/>.
/// </summary>
/// <param name="name">The name of the setting.</param>
/// <param name="description">The description of the setting.</param>
/// <param name="names">Optional custom <see langword="enum"/> names.</param>
[AttributeUsage(AttributeTargets.Property)]
public class LocalEnumSettingAttribute(
    string? name = null,
    string? description = null,
    string[]? names = null
    ) : LocalSettingAttribute(name, description)
{
    private readonly string? _name = name;
    private readonly string? _description = description;

    /// <inheritdoc/>
    public override LocalEnumSetting CreateSetting(Type tab, ConfigEntryBase configEntryBase)
    {
        return new LocalEnumSetting(tab, configEntryBase, configEntryBase.SettingType, _name, _description, names);
    }
}

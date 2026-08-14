using System;
using BepInEx.Configuration;
using MiraAPI.LocalSettings.SettingTypes;
using MiraAPI.Utilities;

namespace MiraAPI.LocalSettings.Attributes;

/// <summary>
/// Creates a <see cref="LocalNumberSetting"/> for the <see cref="ConfigEntry{T}"/>.
/// </summary>
/// <param name="name">The name of the setting.</param>
/// <param name="description">The description of the setting.</param>
/// <param name="min">Minimum range.</param>
/// <param name="max">Maximum range.</param>
/// <param name="increment">Increment per use.</param>
/// <param name="suffixType">Suffix for the value.</param>
/// <param name="formatString">Format string used when formatting.</param>
/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property)]
public class LocalNumberSettingAttribute(
    string? name = null,
    string? description = null,
    float min = 1,
    float max = 5,
    float increment = 1,
    MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
    string? formatString = null
    ) : LocalSettingAttribute(name, description)
{
    private readonly string? _name = name;
    private readonly string? _description = description;

    /// <inheritdoc/>
    public override LocalNumberSetting CreateSetting(Type tab, ConfigEntryBase configEntryBase)
    {
        return new LocalNumberSetting(tab, configEntryBase, _name, _description, new FloatRange(min, max), increment, suffixType, formatString);
    }
}

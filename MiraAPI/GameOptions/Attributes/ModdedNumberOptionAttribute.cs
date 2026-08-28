using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using MiraAPI.Utilities;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a <see cref="ModdedNumberOption"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedNumberOptionAttribute(
    string title,
    float min,
    float max,
    float increment = 1,
    MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
    string? formatString = null,
    bool zeroInfinity = false)
    : ModdedOptionAttribute(title)
{
    internal override IModdedOption CreateOption(object? value, PropertyInfo property)
    {
        return new ModdedNumberOption(
            Title.Translate(),
            (float)(value ?? min + increment),
            min,
            max,
            increment,
            suffixType,
            formatString,
            zeroInfinity);
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        var opt = HolderOption as ModdedNumberOption;
        opt?.SetValue((float)value);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return HolderOption is ModdedNumberOption opt
            ? (object)opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedNumberOption");
    }
}

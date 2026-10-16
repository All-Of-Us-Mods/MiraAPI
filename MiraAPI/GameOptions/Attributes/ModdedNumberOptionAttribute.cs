using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a <see cref="ModdedNumberOption"/>.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="min">The minimum value.</param>
/// <param name="max">The maximum value.</param>
/// <param name="increment">The increment.</param>
/// <param name="suffixType">The suffix type.</param>
/// <param name="formatString">Optional format string for the option screen.</param>
/// <param name="zeroInfinity">Flag indicating whether 0 means infinity.</param>
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
        var final = value switch
        {
            null => min + increment,
            int integer => integer,
            _ => (float)value,
        };

        return new ModdedNumberOption(
            Title,
            final,
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
        if (HolderOption is not ModdedNumberOption opt)
            return;

        var final = value switch
        {
            int integer => integer,
            _ => (float)value,
        };
        opt.SetValue(final);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        if (HolderOption is not ModdedNumberOption opt)
            throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedNumberOption");

        var value = opt.Value;

        if (ValueType == typeof(int))
            return (int)Math.Round(value);

        return value;
    }
}

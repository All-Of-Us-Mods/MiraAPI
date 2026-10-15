using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a <see cref="ModdedStringOption"/>.
/// </summary>
/// <param name="title">The title of the option.</param>
/// <param name="values">An option list of string value.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedStringOptionAttribute(
    string title,
    string[] values)
    : ModdedOptionAttribute(title)
{
    internal override IModdedOption? CreateOption(object? value, PropertyInfo property)
    {
        var option = new ModdedStringOption(Title, (string)(value ?? string.Empty), values);
        return option;
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return HolderOption is ModdedStringOption opt
            ? (object)opt.Value
            : throw new InvalidOperationException($"Holder option for {Title} is not a ModdedStringOption.");
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        var opt = HolderOption as ModdedStringOption;
        opt?.SetValue((string)value);
    }
}

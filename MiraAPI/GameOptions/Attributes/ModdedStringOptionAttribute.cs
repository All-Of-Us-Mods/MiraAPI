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
public class ModdedStringOptionAttribute(string? title, string[] values)
    : ModdedOptionAttribute(title), ITypedOption<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedStringOptionAttribute"/> class.
    /// </summary>
    /// <param name="values">An option list of string value.</param>
    public ModdedStringOptionAttribute(string[] values)
        : this(null, values)
    {
    }

    internal override IModdedOption CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group)
    {
        var opt = new ModdedStringOption(group.GroupPrefix, Title ?? property.Name, value as string ?? string.Empty, values);
        Title = opt.Title;
        return opt;
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return GetValueUnboxed();
    }

    /// <inheritdoc />
    public string GetValueUnboxed()
    {
        return HolderOption is ModdedStringOption opt
            ? opt.Value
            : throw new InvalidOperationException($"Holder option for {Title} is not a ModdedStringOption.");
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        SetValueUnboxed((string)value);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(string value)
    {
        (HolderOption as ModdedStringOption)?.SetValue(value);
    }
}

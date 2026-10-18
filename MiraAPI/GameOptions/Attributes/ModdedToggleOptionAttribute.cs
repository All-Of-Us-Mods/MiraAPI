using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a <see cref="ModdedToggleOption"/>.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="on">What the option displays when toggled on.</param>
/// <param name="off">What the option displays when toggled off.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedToggleOptionAttribute(
    string title,
    string on,
    string off)
    : ModdedOptionAttribute(title)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionAttribute"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    public ModdedToggleOptionAttribute(string title)
        : this(title, "On", "Off")
    {
    }

    internal override IModdedOption CreateOption(object? value, PropertyInfo property)
    {
        var toggleOpt = new ModdedToggleOption(Title, (bool)(value ?? false), on, off);
        return toggleOpt;
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        var opt = HolderOption as ModdedToggleOption;
        opt?.SetValue((bool)value);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return HolderOption is ModdedToggleOption opt
            ? opt.Value
            : throw new InvalidOperationException($"Holder option for {Title} is not a ModdedToggleOption.");
    }
}

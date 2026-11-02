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
public class ModdedToggleOptionAttribute(string? title, string on, string off)
    : ModdedOptionAttribute(title), ITypedOption<bool>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionAttribute"/> class.
    /// </summary>
    /// <param name="on">What the option displays when toggled on.</param>
    /// <param name="off">What the option displays when toggled off.</param>
    public ModdedToggleOptionAttribute(string on, string off)
        : this(null, on, off)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionAttribute"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    public ModdedToggleOptionAttribute(string? title)
        : this(title, "On", "Off")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionAttribute"/> class.
    /// </summary>
    public ModdedToggleOptionAttribute()
        : this(null, "On", "Off")
    {
    }

    internal override IModdedOption CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group)
    {
        var opt = new ModdedToggleOption(group.GroupPrefix, Title ?? property.Name, (bool)(value ?? false), on, off);
        Title = opt.Title;
        return opt;
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        SetValueUnboxed((bool)value);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(bool value)
    {
        (HolderOption as ModdedToggleOption)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return GetValueUnboxed();
    }

    /// <inheritdoc />
    public bool GetValueUnboxed()
    {
        return HolderOption is ModdedToggleOption opt
            ? opt.Value
            : throw new InvalidOperationException($"Holder option for {Title} is not a ModdedToggleOption.");
    }
}

using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a <see cref="ModdedToggleOption"/>.
/// </summary>
/// <param name="title">The option title.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedToggleOptionAttribute(string title) : ModdedOptionAttribute(title)
{
    internal override IModdedOption CreateOption(object? value, PropertyInfo property)
    {
        var toggleOpt = new ModdedToggleOption(Title, (bool)(value ?? false));
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
            ? (object)opt.Value
            : throw new InvalidOperationException($"Holder option for {Title} is not a ModdedToggleOption.");
    }
}

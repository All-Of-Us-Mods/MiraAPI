using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a list of toggle options.
/// </summary>
/// <param name="title">The option's title.</param>
/// <param name="on">What the option displays when toggled on.</param>
/// <param name="off">What the option displays when toggled off.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedToggleOptionListAttribute(
    string title,
    string on,
    string off)
    : ModdedOptionListAttribute(title)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionListAttribute"/> class.
    /// </summary>
    /// <param name="title">The option's title.</param>
    public ModdedToggleOptionListAttribute(string title)
        : this(title, "On", "Off")
    {
    }

    internal override IModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        var optList = new ModdedOptionList<ModdedToggleOption>(
            value.Count, idx => new(GetFormattedTitle(idx), (bool)(value[idx] ?? false), on, off));
        return optList;
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        var opt = HolderOptionList?[idx] as ModdedToggleOption;
        opt?.SetValue((bool)value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        return HolderOptionList?[idx] is ModdedToggleOption opt
            ? opt.Value
            : throw new InvalidOperationException($"Holder option for {GetFormattedTitle(idx)} is not a ModdedToggleOption.");
    }
}

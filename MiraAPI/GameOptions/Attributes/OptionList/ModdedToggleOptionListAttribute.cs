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
public class ModdedToggleOptionListAttribute(string? title, string on, string off)
    : ModdedOptionListAttribute(title), ITypedOptionList<bool>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionListAttribute"/> class.
    /// </summary>
    /// <param name="on">What the option displays when toggled on.</param>
    /// <param name="off">What the option displays when toggled off.</param>
    public ModdedToggleOptionListAttribute(string on, string off)
        : this(null, on, off)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionListAttribute"/> class.
    /// </summary>
    /// <param name="title">The option's title.</param>
    public ModdedToggleOptionListAttribute(string? title)
        // ReSharper disable once IntroduceOptionalParameters.Global (Justification: Kept for backwards compatibility.)
        : this(title, "On", "Off")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOptionListAttribute"/> class.
    /// </summary>
    public ModdedToggleOptionListAttribute()
        : this(null, "On", "Off")
    {
    }

    internal override IModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        var optList = new ModdedOptionList<ModdedToggleOption>(
            list.Count, idx => new(group.GroupPrefix, GetFormattedTitle(idx, property), (bool)(list[idx] ?? false), on, off));
        return optList;
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        SetValueUnboxed(idx, (bool)value);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(int idx, bool value)
    {
        (HolderOptionList?[idx] as ModdedToggleOption)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        return GetValueUnboxed(idx);
    }

    /// <inheritdoc />
    public bool GetValueUnboxed(int idx)
    {
        return HolderOptionList?[idx] is ModdedToggleOption opt
            ? opt.Value
            : throw new InvalidOperationException($"Holder option \"{Titles[idx]}\" is not a ModdedToggleOption.");
    }
}

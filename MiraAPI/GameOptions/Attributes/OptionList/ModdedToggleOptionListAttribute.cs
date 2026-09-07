using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Collections;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a list of toggle options.
/// </summary>
/// <param name="title">The option's title.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedToggleOptionListAttribute(string title) : ModdedOptionListAttribute(title)
{
    internal override IModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        var optList = new ModdedOptionList<ModdedToggleOption>(
            value.Count, idx => new(GetFormattedTitle(idx), (bool)(value[idx] ?? false)));
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
        if (HolderOptionList?[idx] is ModdedToggleOption opt)
        {
            return opt.Value;
        }
        throw new InvalidOperationException($"Holder option for {GetFormattedTitle(idx)} is not a ModdedToggleOption.");
    }
}

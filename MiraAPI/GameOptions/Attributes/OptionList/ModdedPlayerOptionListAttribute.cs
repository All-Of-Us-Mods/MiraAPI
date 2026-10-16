using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of player options.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="allowNone">Whether the option includes a none option. If None is selected, value will return -1.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedPlayerOptionListAttribute(
    string title,
    bool allowNone = true)
    : ModdedOptionListAttribute(title)
{
    internal override IModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        return new ModdedOptionList<ModdedPlayerOption>(value.Count, idx =>
        {
            return new(GetFormattedTitle(idx), allowNone);
        });
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        if (HolderOptionList?[idx] is not ModdedPlayerOption opt)
            return;

        var final = value switch
        {
            NetworkedPlayerInfo info => opt.Values.IndexOf(info),
            _ => (int)value,
        };
        opt?.SetValue(final);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        if (HolderOptionList?[idx] is not ModdedPlayerOption opt)
            throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedPlayerOption");

        if (ValueType == typeof(int))
            return opt.Value;

        return opt.SelectedPlayer!;
    }
}

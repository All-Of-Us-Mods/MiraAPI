using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of string options.
/// </summary>
/// <param name="title">The title of the option.</param>
/// <param name="values">An option list of string value.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedStringOptionListAttribute(
    string title,
    string[] values)
    : ModdedOptionListAttribute(title)
{
    internal override IBaseModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        return new ModdedOptionList<ModdedStringOption>(value.Count, idx =>
        {
            return new ModdedStringOption(Title.Translate(), (string)(value[idx] ?? string.Empty), values);
        });
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        var opt = HolderOptionList?.ElementAt(idx) as ModdedStringOption;
        opt?.SetValue((string)value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        return HolderOptionList?.ElementAt(idx) is ModdedStringOption opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedStringOption");
    }
}

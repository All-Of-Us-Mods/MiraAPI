using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of string options.
/// </summary>
/// <param name="title">The title of the option.</param>
/// <param name="values">An option list of string value.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedStringOptionListAttribute(string? title, string[] values)
    : ModdedOptionListAttribute(title), ITypedOptionList<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedStringOptionListAttribute"/> class.
    /// </summary>
    /// <param name="values">An option list of string value.</param>
    public ModdedStringOptionListAttribute(string[] values)
        : this(null, values)
    {
    }

    internal override IModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        return new ModdedOptionList<ModdedStringOption>(list.Count, idx =>
        {
            return new ModdedStringOption(group.GroupPrefix, GetFormattedTitle(idx, property), (string)(list[idx] ?? string.Empty), values);
        });
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        SetValueUnboxed(idx, (string)value);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(int idx, string value)
    {
        (HolderOptionList?[idx] as ModdedStringOption)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        return GetValueUnboxed(idx);
    }

    /// <inheritdoc />
    public string GetValueUnboxed(int idx)
    {
        return HolderOptionList?[idx] is ModdedStringOption opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedStringOption");
    }
}

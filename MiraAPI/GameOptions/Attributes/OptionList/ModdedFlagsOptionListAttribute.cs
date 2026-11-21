using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of flags options.
/// </summary>
/// <typeparam name="T">The flag enum type.</typeparam>
/// <param name="title">The option's title.</param>
/// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
/// <param name="allowedValues">An option array of enum values to filter down what values can actually be used.</param>
/// <param name="values">An optional array of string values to use in place of the <see langword="enum"/> name.</param>
/// <param name="noneId">The core part of the translation ID corresponding to when no options are selected.</param>
/// <param name="allId">The core part of the translation ID corresponding to when all options are selected.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedFlagsOptionListAttribute<T>(string? title, string idPrefix, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null)
    : ModdedOptionListAttribute(title), ITypedOptionList<T>
    where T : struct, Enum
{
    /// <inheritdoc cref="ModdedFlagsOptionListAttribute{T}(string, string, T[], string[], string, string)"/>
    public ModdedFlagsOptionListAttribute(string? title, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null)
        : this(title, string.Empty, allowedValues, values, noneId, allId)
    {
    }

    /// <inheritdoc cref="ModdedFlagsOptionListAttribute{T}(string, string, T[], string[], string, string)"/>
    public ModdedFlagsOptionListAttribute(T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, string idPrefix = "")
        : this(null, idPrefix, allowedValues, values, noneId, allId)
    {
    }

    /// <inheritdoc/>
    internal override IBaseModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        var optList = new ModdedOptionList<ModdedFlagsOption<T>>(
            list.Count, idx => new(group.GroupPrefix, GetFormattedTitle(idx, property), (T)(list[idx] ?? 0), idPrefix, allowedValues, values, noneId, allId));
        return optList;
    }

    /// <inheritdoc/>
    public override void SetValue(int idx, object value)
    {
        SetValueUnboxed(idx, (T)value);
    }

    /// <inheritdoc/>
    public void SetValueUnboxed(int idx, T value)
    {
        (HolderOptionList?.ElementAt(idx) as ModdedFlagsOption<T>)?.SetValue(value);
    }

    /// <inheritdoc/>
    public override object GetValue(int idx)
    {
        return GetValueUnboxed(idx);
    }

    /// <inheritdoc/>
    public T GetValueUnboxed(int idx)
    {
        return HolderOptionList?.ElementAt(idx) is ModdedFlagsOption<T> opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedFlagsOption<{typeof(T).FullName}>");
    }
}

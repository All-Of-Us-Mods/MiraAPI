using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of enum options.
/// </summary>
/// <param name="title">The title of the option.</param>
/// <param name="enumType">The <see cref="Enum"/> type.</param>
/// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
[AttributeUsage(AttributeTargets.Property)]
[Obsolete("Use ModdedEnumOptionListAttribute<T> instead.")]
public class ModdedEnumOptionListAttribute(string title, Type enumType, string[]? values = null)
    : ModdedOptionListAttribute(title)
{
    internal override IModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        var optList = new ModdedOptionList<ModdedEnumOption>(
            value.Count, idx => new(GetFormattedTitle(idx), (int)(value[idx] ?? 0), enumType, values));
        return optList;
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        var opt = HolderOptionList?[idx] as ModdedEnumOption;
        opt?.SetValue((int)value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        return HolderOptionList?[idx] is ModdedEnumOption opt
            ? Enum.ToObject(enumType, opt.Value)
            : throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedEnumOption with enum type {enumType.FullName}");
    }
}

/// <summary>
/// Attribute for creating a list of enum options.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
/// <param name="title">The option's title.</param>
/// <param name="values">The option's possible values.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedEnumOptionListAttribute<T>(string title, string[]? values = null)
    : ModdedOptionListAttribute(title), ITypedOptionList<T>
    where T : struct, Enum
{
    internal override IModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        var optList = new ModdedOptionList<ModdedEnumOption<T>>(
            value.Count, idx => new(GetFormattedTitle(idx), (T)(value[idx] ?? 0), values));
        return optList;
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        SetValueUnboxed(idx, (T)value);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(int idx, T value)
    {
        (HolderOptionList?[idx] as ModdedEnumOption<T>)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        return GetValueUnboxed(idx);
    }

    /// <inheritdoc />
    public T GetValueUnboxed(int idx)
    {
        return HolderOptionList?[idx] is ModdedEnumOption<T> opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedEnumOption<{typeof(T).FullName}>");
    }
}

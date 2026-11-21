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
/// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
/// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
[AttributeUsage(AttributeTargets.Property)]
[Obsolete("Use ModdedEnumOptionListAttribute<T> instead. This will be removed in a future version.")]
public class ModdedEnumOptionListAttribute(string? title, Type enumType, string idPrefix, string[]? values = null)
    : ModdedOptionListAttribute(title)
{
    /// <inheritdoc cref="ModdedEnumOptionListAttribute(string, Type, string, string[])"/>
    public ModdedEnumOptionListAttribute(string? title, Type enumType, string[]? values = null)
        : this(title, enumType, string.Empty, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionListAttribute(string, Type, string, string[])"/>
    public ModdedEnumOptionListAttribute(Type enumType, string idPrefix, string[]? values = null)
        : this(null, enumType, idPrefix, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionListAttribute(string, Type, string, string[])"/>
    public ModdedEnumOptionListAttribute(Type enumType, string[]? values = null)
        : this(null, enumType, string.Empty, values)
    {
    }

    internal override IBaseModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        var optList = new ModdedOptionList<ModdedEnumOption>(
            list.Count, idx => new(group.GroupPrefix, GetFormattedTitle(idx, property), (int)(list[idx] ?? 0), enumType, idPrefix, values));
        return optList;
    }

    /// <inheritdoc/>
    public override void SetValue(int idx, object value)
    {
        var opt = HolderOptionList?.ElementAt(idx) as ModdedEnumOption;
        opt?.SetValue((int)value);
    }

    /// <inheritdoc/>
    public override object GetValue(int idx)
    {
        return HolderOptionList?.ElementAt(idx) is ModdedEnumOption opt
            ? Enum.ToObject(enumType, opt.UnderlyingValues[opt.Value])
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedEnumOption with enum type {enumType.FullName}");
    }
}

/// <summary>
/// Attribute for creating a list of enum options.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
/// <param name="title">The option's title.</param>
/// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
/// <param name="allowedValues">An option array of enum values to filter down what values can actually be used.</param>
/// <param name="values">An optional array of string values to use in place of the <see langword="enum"/> name.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedEnumOptionListAttribute<T>(string? title, string idPrefix, T[]? allowedValues = null, string[]? values = null)
    : ModdedOptionListAttribute(title), ITypedOptionList<T>
    where T : struct, Enum
{
    private const string ObsoleteMsg = "This constructor is deprecated. Please use the equivalent constructor that includes the 'T[]? allowedValues' parameter. This will be removed in a future version.";

    /// <inheritdoc cref="ModdedEnumOptionListAttribute{T}(string, string, T[], string[])"/>
    public ModdedEnumOptionListAttribute(string? title, T[]? allowedValues = null, string[]? values = null)
        : this(title, string.Empty, allowedValues, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionListAttribute{T}(string, string, T[], string[])"/>
    public ModdedEnumOptionListAttribute(T[]? allowedValues = null, string[]? values = null, string idPrefix = "")
        : this(null, idPrefix, allowedValues, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionListAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionListAttribute(string? title, string idPrefix, string[] values)
        : this(title, idPrefix, null, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionListAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionListAttribute(string? title, string[] values)
        : this(title, string.Empty, null, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionListAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionListAttribute(string[] values, string idPrefix)
        : this(null, idPrefix, null, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionListAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionListAttribute(string[] values)
        : this(null, string.Empty, null, values)
    {
    }

    /// <inheritdoc/>
    internal override IBaseModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        var optList = new ModdedOptionList<ModdedEnumOption<T>>(
            list.Count, idx => new(group.GroupPrefix, GetFormattedTitle(idx, property), (T)(list[idx] ?? 0), idPrefix, allowedValues, values));
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
        (HolderOptionList?.ElementAt(idx) as ModdedEnumOption<T>)?.SetValue(value);
    }

    /// <inheritdoc/>
    public override object GetValue(int idx)
    {
        return GetValueUnboxed(idx);
    }

    /// <inheritdoc/>
    public T GetValueUnboxed(int idx)
    {
        return HolderOptionList?.ElementAt(idx) is ModdedEnumOption<T> opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedEnumOption<{typeof(T).FullName}>");
    }
}

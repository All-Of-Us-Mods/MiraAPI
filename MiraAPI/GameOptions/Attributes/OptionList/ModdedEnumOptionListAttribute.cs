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
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOptionListAttribute"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    public ModdedEnumOptionListAttribute(string? title, Type enumType, string[]? values = null)
        : this(title, enumType, string.Empty, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOptionListAttribute"/> class.
    /// </summary>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    public ModdedEnumOptionListAttribute(Type enumType, string idPrefix, string[]? values = null)
        : this(null, enumType, idPrefix, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOptionListAttribute"/> class.
    /// </summary>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    public ModdedEnumOptionListAttribute(Type enumType, string[]? values = null)
        : this(null, enumType, string.Empty, values)
    {
    }

    internal override IModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        var optList = new ModdedOptionList<ModdedEnumOption>(
            list.Count, idx => new(group.GroupPrefix, GetFormattedTitle(idx, property), (int)(list[idx] ?? 0), enumType, idPrefix, values));
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
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedEnumOption with enum type {enumType.FullName}");
    }
}

/// <summary>
/// Attribute for creating a list of enum options.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
/// <param name="title">The option's title.</param>
/// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
/// <param name="values">The option's possible values.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedEnumOptionListAttribute<T>(string? title, string idPrefix, string[]? values = null)
    : ModdedOptionListAttribute(title), ITypedOptionList<T>
    where T : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOptionListAttribute{T}"/> class.
    /// </summary>
    /// <param name="title">The option's title.</param>
    /// <param name="values">The option's possible values.</param>
    public ModdedEnumOptionListAttribute(string? title, string[]? values = null)
        : this(title, string.Empty, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOptionListAttribute{T}"/> class.
    /// </summary>
    /// <param name="values">The option's possible values.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    public ModdedEnumOptionListAttribute(string[]? values = null, string idPrefix = "")
        : this(null, idPrefix, values)
    {
    }

    internal override IModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        var optList = new ModdedOptionList<ModdedEnumOption<T>>(
            list.Count, idx => new(group.GroupPrefix, GetFormattedTitle(idx, property), (T)(list[idx] ?? 0), idPrefix, values));
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
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedEnumOption<{typeof(T).FullName}>");
    }
}

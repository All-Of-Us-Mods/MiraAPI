using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a <see cref="ModdedEnumOption"/>.
/// </summary>
/// <param name="title">The title of the option.</param>
/// <param name="enumType">The <see cref="Enum"/> type.</param>
/// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
/// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
[AttributeUsage(AttributeTargets.Property)]
[Obsolete("Use ModdedEnumOptionAttribute<T> instead. This will be removed in a future version.")]
public class ModdedEnumOptionAttribute(string? title, Type enumType, string idPrefix, string[]? values = null)
    : ModdedOptionAttribute(title)
{
    /// <inheritdoc cref="ModdedEnumOptionAttribute(string, Type, string, string[])"/>
    public ModdedEnumOptionAttribute(string? title, Type enumType, string[]? values = null)
        : this(title, enumType, string.Empty, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionAttribute(string, Type, string, string[])"/>
    public ModdedEnumOptionAttribute(Type enumType, string idPrefix, string[]? values = null)
        : this(null, enumType, idPrefix, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionAttribute(string, Type, string, string[])"/>
    public ModdedEnumOptionAttribute(Type enumType, string[]? values = null)
        : this(null, enumType, string.Empty, values)
    {
    }

    internal override IModdedOption CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group)
    {
        var opt = new ModdedEnumOption(group.GroupPrefix, Title ?? property.Name, (int)(value ?? 0), enumType, idPrefix, values);
        Title = opt.Title;
        return opt;
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        (HolderOption as ModdedEnumOption)?.SetValue((int)value);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return HolderOption is ModdedEnumOption opt
            ? Enum.ToObject(enumType, opt.UnderlyingValues[opt.Value])
            : throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedEnumOption with enum type {enumType.FullName}");
    }
}

/// <summary>
/// Attribute for creating an enum option.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
/// <param name="title">The title of the option.</param>
/// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
/// <param name="allowedValues">An option array of enum values to filter down what values can actually be used.</param>
/// <param name="values">An optional array of string values to use in place of the <see langword="enum"/> name.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedEnumOptionAttribute<T>(string? title, string idPrefix, T[]? allowedValues = null, string[]? values = null)
    : ModdedOptionAttribute(title), ITypedOption<T>
    where T : struct, Enum
{
    private const string ObsoleteMsg = "This constructor is deprecated. Please use the equivalent constructor that includes the 'T[]? allowedValues' parameter. This will be removed in a future version.";

    /// <inheritdoc cref="ModdedEnumOptionAttribute{T}(string, string, T[], string[])"/>
    public ModdedEnumOptionAttribute(string? title, T[]? allowedValues = null, string[]? values = null)
        : this(title, string.Empty, allowedValues, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionAttribute{T}(string, string, T[], string[])"/>
    public ModdedEnumOptionAttribute(T[]? allowedValues = null, string[]? values = null, string idPrefix = "")
        : this(null, idPrefix, allowedValues, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionAttribute(string? title, string idPrefix, string[] values)
        : this(title, idPrefix, null, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionAttribute(string? title, string[] values)
        : this(title, string.Empty, null, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionAttribute(string[] values, string idPrefix)
        : this(null, idPrefix, null, values)
    {
    }

    /// <inheritdoc cref="ModdedEnumOptionAttribute{T}(string, string, T[], string[])"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOptionAttribute(string[] values)
        : this(null, string.Empty, null, values)
    {
    }

    /// <inheritdoc />
    internal override IModdedOption CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group)
    {
        var opt = new ModdedEnumOption<T>(group.GroupPrefix, Title ?? property.Name, (T)(value ?? 0), idPrefix, allowedValues, values);
        Title = opt.Title;
        return opt;
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        SetValueUnboxed((T)value);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(T value)
    {
        (HolderOption as ModdedEnumOption<T>)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return GetValueUnboxed();
    }

    /// <inheritdoc />
    public T GetValueUnboxed()
    {
        return HolderOption is ModdedEnumOption<T> opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedEnumOption<{typeof(T).Name}>");
    }
}

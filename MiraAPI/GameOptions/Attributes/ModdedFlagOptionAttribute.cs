using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a flags option.
/// </summary>
/// <typeparam name="T">The flag enum type.</typeparam>
/// <param name="title">The title of the option.</param>
/// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
/// <param name="allowedValues">An option array of enum values to filter down what values can actually be used.</param>
/// <param name="values">An optional array of string values to use in place of the <see langword="enum"/> name.</param>
/// <param name="noneId">The core part of the translation ID corresponding to when no options are selected.</param>
/// <param name="allId">The core part of the translation ID corresponding to when all options are selected.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedFlagsOptionAttribute<T>(string? title, string idPrefix, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null)
    : ModdedOptionAttribute(title), ITypedOption<T>
    where T : struct, Enum
{
    /// <inheritdoc cref="ModdedFlagsOptionAttribute{T}(string, string, T[], string[], string, string)"/>
    public ModdedFlagsOptionAttribute(string? title, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null)
        : this(title, string.Empty, allowedValues, values, noneId, allId)
    {
    }

    /// <inheritdoc cref="ModdedFlagsOptionAttribute{T}(string, string, T[], string[], string, string)"/>
    public ModdedFlagsOptionAttribute(T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, string idPrefix = "")
        : this(null, idPrefix, allowedValues, values, noneId, allId)
    {
    }

    /// <inheritdoc/>
    internal override IModdedOption CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group)
    {
        var opt = new ModdedFlagsOption<T>(group.GroupPrefix, Title ?? property.Name, (T)(value ?? 0), idPrefix, allowedValues, values, noneId, allId);
        Title = opt.Title;
        return opt;
    }

    /// <inheritdoc/>
    public override void SetValue(object value)
    {
        SetValueUnboxed((T)value);
    }

    /// <inheritdoc/>
    public void SetValueUnboxed(T value)
    {
        (HolderOption as ModdedFlagsOption<T>)?.SetValue(value);
    }

    /// <inheritdoc/>
    public override object GetValue()
    {
        return GetValueUnboxed();
    }

    /// <inheritdoc/>
    public T GetValueUnboxed()
    {
        return HolderOption is ModdedFlagsOption<T> opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedFlagsOption<{typeof(T).Name}>");
    }
}

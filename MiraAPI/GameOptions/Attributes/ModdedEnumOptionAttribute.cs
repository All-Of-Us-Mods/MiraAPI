using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a <see cref="ModdedEnumOption"/>.
/// </summary>
/// <param name="title">The title of the option.</param>
/// <param name="enumType">The <see cref="Enum"/> type.</param>
/// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
[AttributeUsage(AttributeTargets.Property)]
[Obsolete("Use ModdedEnumOptionAttribute<T> instead. This will be removed in a future version.")]
public class ModdedEnumOptionAttribute(string title, Type enumType, string[]? values = null)
    : ModdedOptionAttribute(title)
{
    internal override IModdedOption CreateOption(object? value, PropertyInfo property)
    {
        var opt = new ModdedEnumOption(Title, (int)(value ?? 0), enumType, values);
        return opt;
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        var opt = HolderOption as ModdedEnumOption;
        opt?.SetValue((int)value);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        return HolderOption is ModdedEnumOption opt
            ? Enum.ToObject(enumType, opt.Value)
            : throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedEnumOption with enum type {enumType.FullName}");
    }
}

/// <summary>
/// Attribute for creating an enum option.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
/// <param name="title">The title of the option.</param>
/// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedEnumOptionAttribute<T>(string title, string[]? values = null)
    : ModdedOptionAttribute(title), ITypedOption<T>
    where T : struct, Enum
{
    internal override IModdedOption CreateOption(object? value, PropertyInfo property)
    {
        var opt = new ModdedEnumOption<T>(Title, (T)(value ?? 0), values);
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

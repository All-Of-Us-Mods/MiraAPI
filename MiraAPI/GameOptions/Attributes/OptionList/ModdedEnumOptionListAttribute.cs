using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Collections;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of enum options.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
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
            : throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" with EnumType ${enumType.FullName} is not a ModdedEnumOption");
    }
}

/// <summary>
/// Attribute for creating a list of enum options.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedEnumOptionListAttribute<T>(string title, string[]? values = null)
    : ModdedOptionListAttribute(title) where T : Enum
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
        var opt = HolderOptionList?[idx] as ModdedEnumOption<T>;
        opt?.SetValue((T)value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        return HolderOptionList?[idx] is ModdedEnumOption<T> opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" with EnumType ${typeof(T).FullName} is not a ModdedEnumOption");
    }
}

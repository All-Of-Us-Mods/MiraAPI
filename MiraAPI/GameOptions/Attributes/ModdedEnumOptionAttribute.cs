using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating an enum option.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedEnumOptionAttribute(string title, Type enumType, string[]? values = null, Type? roleType = null)
    : ModdedOptionAttribute(title, roleType)
{
    internal override IModdedOption CreateOption(object? value, PropertyInfo property)
    {
        var opt = new ModdedEnumOption(Title, (int)(value ?? 0), enumType, values, RoleType);
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
        if (HolderOption is ModdedEnumOption opt)
        {
            return Enum.ToObject(enumType, opt.Value);
        }
        throw new InvalidOperationException($"HolderOption for option \"{Title}\" with EnumType ${enumType.FullName} is not a ModdedEnumOption");
    }
}

using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

public class ModdedEnumOptionAttribute(string title, Type enumType, string[] values = null, Type? roleType = null)
    : ModdedOptionAttribute(title, roleType)
{
    internal override IModdedOption CreateOption(object? value, PropertyInfo property)
    {
        var opt = new ModdedEnumOption(Title, (int)(value ?? 0), enumType, values, RoleType);
        return opt;
    }

    public override void SetValue(object value)
    {
        var opt = HolderOption as ModdedEnumOption;
        opt.SetValue((int)value);
    }

    public override object GetValue()
    {
        var opt = HolderOption as ModdedEnumOption;
        return Enum.ToObject(enumType, opt.Value);
    }
}
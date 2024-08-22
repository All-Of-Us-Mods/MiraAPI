using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

public class ModdedEnumOptionAttribute(string title, Type enumType, Type roleType = null)
    : ModdedOptionAttribute(title, roleType)
{
    internal override IModdedOption CreateOption(object value, PropertyInfo property)
    {
        var opt = new ModdedEnumOption(Title, (int)value, enumType, RoleType);
        return opt;
    }

    public override void SetValue(object value)
    {
        ModdedEnumOption opt = HolderOption as ModdedEnumOption;
        opt.SetValue((int)value);
    }

    public override object GetValue()
    {
        ModdedEnumOption opt = HolderOption as ModdedEnumOption;
        return Enum.ToObject(enumType, opt.Value);
    }
}
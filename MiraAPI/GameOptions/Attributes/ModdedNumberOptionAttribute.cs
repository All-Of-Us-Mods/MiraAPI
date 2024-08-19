using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

public class ModdedNumberOptionAttribute(
    string title,
    float min,
    float max,
    float increment = 1,
    NumberSuffixes suffixType = NumberSuffixes.None,
    bool zeroInfinity = false,
    Type roleType = null)
    : ModdedOptionAttribute(title, roleType)
{
    public override IModdedOption CreateOption(object value, PropertyInfo property)
    {
        var toggleOpt = new ModdedNumberOption(Title, (float)value, min, max, increment, suffixType, zeroInfinity, RoleType);
        return toggleOpt;
    }

    public override void SetValue(object value)
    {
        ModdedNumberOption toggleOpt = HolderOption as ModdedNumberOption;
        toggleOpt.SetValue((float)value);
    }

    public override object GetValue()
    {
        ModdedNumberOption toggleOpt = HolderOption as ModdedNumberOption;
        return toggleOpt.Value;
    }
}
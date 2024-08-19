using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

public class ModdedNumberOptionAttribute : ModdedOptionAttribute
{
    public float Min { get; private set; }
    public float Max { get; private set; }
    public float Increment { get; private set; }
    public NumberSuffixes SuffixType { get; private set; }
    public bool ZeroInfinity { get; private set; }

    public ModdedNumberOptionAttribute(string title, float min, float max, float increment = 1, NumberSuffixes suffixType = NumberSuffixes.None, bool zeroInfinity = false, Type roleType = null) : base(title, roleType)
    {
        Min = min;
        Max = max;
        Increment = increment;
        SuffixType = suffixType;
        ZeroInfinity = zeroInfinity;
    }

    public override IModdedOption CreateOption(object value, PropertyInfo property)
    {
        var toggleOpt = new ModdedNumberOption(Title, (float)value, Min, Max, Increment, SuffixType, ZeroInfinity, RoleType);
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
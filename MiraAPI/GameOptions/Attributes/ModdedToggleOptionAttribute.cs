using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

public class ModdedToggleOptionAttribute(string title, Type roleType = null) : ModdedOptionAttribute(title, roleType)
{
    public override IModdedOption CreateOption(object value, PropertyInfo property)
    {
        var toggleOpt = new ModdedToggleOption(Title, (bool)value, RoleType);
        return toggleOpt;
    }

    public override void SetValue(object value)
    {
        ModdedToggleOption toggleOpt = HolderOption as ModdedToggleOption;
        toggleOpt.SetValue((bool)value);
    }

    public override object GetValue()
    {
        ModdedToggleOption toggleOpt = HolderOption as ModdedToggleOption;
        return toggleOpt.Value;
    }
}
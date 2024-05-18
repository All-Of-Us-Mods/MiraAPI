using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;
using UnityEngine;

namespace MiraAPI.GameOptions.Attributes
{
    public class ModdedToggleOptionAttribute : ModdedOptionAttribute
    {
        public ModdedToggleOptionAttribute(string title, Type roleType = null) : base(title, roleType)
        {
        }

        public override IModdedOption CreateOption(object value, PropertyInfo property)
        {
            Debug.LogError(value.ToString());
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
}

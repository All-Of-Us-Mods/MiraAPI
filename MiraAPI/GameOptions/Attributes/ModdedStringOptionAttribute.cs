using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes
{
    public class ModdedStringOptionAttribute : ModdedOptionAttribute
    {
        public string[] Values;
        public ModdedStringOptionAttribute(string title, string[] strings, Type roleType = null) : base(title, roleType)
        {
            Values = strings;
        }

        public override IModdedOption CreateOption(object value, PropertyInfo property)
        {
            var opt = new ModdedStringOption(Title, (int)value, Values, RoleType);
            return opt;
        }

        public override void SetValue(object value)
        {
            ModdedStringOption opt = HolderOption as ModdedStringOption;
            opt.SetValue((int)value);
        }

        public override object GetValue()
        {
            ModdedStringOption opt = HolderOption as ModdedStringOption;
            return opt.Value;
        }
    }
}

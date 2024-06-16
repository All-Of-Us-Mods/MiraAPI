using Reactor.Localization.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedEnumOption : ModdedOption<int>
    {
        public string[] Values;

        public ModdedEnumOption(string title, int defaultValue, Type enumType, System.Type roleType) : base(title, defaultValue, roleType)
        {
            Values = Enum.GetNames(enumType);
        }

        public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
        {
            var stringOption = UnityEngine.Object.Instantiate(stringOpt, container);

            stringOption.name = Title;
            stringOption.Title = StringName;
            stringOption.Value = Value;
            stringOption.Values = Values.Select(CustomStringName.CreateAndRegister).ToArray();
            stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
            stringOption.OnEnable();

            OptionBehaviour = stringOption;

            return stringOption;
        }

        public override string GetHudStringText()
        {
            return Title + ": " + Values[Value];
        }

        public override int GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
        {
            return optionBehaviour.GetInt();
        }

        public override void OnValueChanged(int newValue)
        {
            if (OptionBehaviour is null) return;

            StringOption opt = OptionBehaviour as StringOption;
            opt.Value = newValue;
        }
    }
}

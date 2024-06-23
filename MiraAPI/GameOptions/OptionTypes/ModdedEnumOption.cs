using Reactor.Localization.Utilities;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedEnumOption : ModdedOption<int>
    {
        public string[] Values;

        public ModdedEnumOption(string title, int defaultValue, Type enumType, Type roleType) : base(title, defaultValue, roleType)
        {
            Values = Enum.GetNames(enumType);
        }

        public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
        {
            var stringOption = Object.Instantiate(stringOpt, container);
            var vals = Values.Select(CustomStringName.CreateAndRegister).ToArray();
            var data = ScriptableObject.CreateInstance<StringGameSetting>();

            data.Title = StringName;
            data.Type = global::OptionTypes.String;
            data.Values = vals;
            data.Index = Value;

            stringOption.data = data;
            stringOption.Title = StringName;
            stringOption.TitleText.text = Title;
            stringOption.Values = vals;

            stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

            OptionBehaviour = stringOption;
            stringOption.Initialize();
            stringOption.Value = Value;
            stringOption.ValueText.text = Values[Value];

            stringOption.SetUpFromData(stringOption.data, 20);
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

            var opt = OptionBehaviour as StringOption;
            opt.Value = newValue;

            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringName, OptionBehaviour.GetValueString(Value), false);
        }
    }
}

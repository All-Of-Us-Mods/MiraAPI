using Reactor.Localization.Utilities;
using System;
using System.Linq;
using MiraAPI.Networking;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedStringOption(string title, int defaultValue, string[] values, Type roleType) : ModdedOption<int>(title, defaultValue, roleType)
    {
        public string[] Values { get; private set; } = values;

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
            stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

            OptionBehaviour = stringOption;

            stringOption.SetUpFromData(stringOption.data, 20);

            stringOption.Value = Value;

            return stringOption;
        }

        public override NetData GetNetData()
        {
            return new NetData(Id, BitConverter.GetBytes(Value));
        }

        public override void HandleNetData(byte[] data)
        {
            SetValue(BitConverter.ToInt32(data));
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
            if (opt)
            {
                opt.Value = newValue;
            }

            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringName, OptionBehaviour.GetValueString(Value), false);
        }
    }
}

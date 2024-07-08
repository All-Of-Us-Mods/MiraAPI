using System;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedNumberOption : ModdedOption<float>
    {
        public float Min { get; private set; }
        public float Max { get; private set; }
        public float Increment { get; private set; }
        public NumberSuffixes SuffixType { get; private set; }
        public bool ZeroInfinity { get; private set; }
        public ModdedNumberOption(string title, float defaultValue, float min, float max, float increment, NumberSuffixes suffixType, bool zeroInfinity, System.Type roleType) : base(title, defaultValue, roleType)
        {
            Min = min;
            Max = max;
            Increment = increment;
            SuffixType = suffixType;
            ZeroInfinity = zeroInfinity;

            Value = Mathf.Clamp(defaultValue, min, max);
        }

        public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
        {
            var numberOption = Object.Instantiate(numberOpt, Vector3.zero, Quaternion.identity, container);
            var data = ScriptableObject.CreateInstance<FloatGameSetting>();
            data.Title = StringName;
            data.Type = global::OptionTypes.Float;
            data.Increment = Increment;
            data.SuffixType = SuffixType;
            data.FormatString = "0";
            data.ValidRange = new FloatRange(Min, Max);
            data.ZeroIsInfinity = ZeroInfinity;
            data.Value = Value;

            numberOption.data = data;
            numberOption.SetUpFromData(numberOption.data, 20);
            numberOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
            OptionBehaviour = numberOption;

            numberOption.Value = Value;

            return numberOption;
        }

        public override NetData GetNetData()
        {
            return new NetData(Id, BitConverter.GetBytes(Value));
        }

        public override void HandleNetData(byte[] data)
        {
            SetValue(BitConverter.ToSingle(data));
        }

        public override string GetHudStringText()
        {
            return Title + ": " + Value + Helpers.GetSuffix(SuffixType);
        }

        public override float GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
        {
            return Mathf.Clamp(optionBehaviour.GetFloat(), Min, Max);
        }

        public override void OnValueChanged(float newValue)
        {
            Value = Mathf.Clamp(newValue, Min, Max);

            if (OptionBehaviour is null) return;

            var opt = OptionBehaviour as NumberOption;
            if (opt)
            {
                opt.Value = newValue;
            }

            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringName, OptionBehaviour.GetValueString(Value), false);
        }
    }
}

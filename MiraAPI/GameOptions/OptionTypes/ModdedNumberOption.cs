using MiraAPI.Utilities;
using UnityEngine;

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

        public override OptionBehaviour CreateOption(OptionBehaviour optionBehaviour, Transform container)
        {
            var numberOption = (NumberOption)Object.Instantiate(optionBehaviour, container);

            numberOption.name = Title;
            numberOption.Title = StringName;
            numberOption.Value = Value;
            numberOption.Increment = Increment;
            numberOption.SuffixType = SuffixType;
            numberOption.FormatString = "0";
            numberOption.ValidRange = new FloatRange(Min, Max);
            numberOption.ZeroIsInfinity = ZeroInfinity;
            numberOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
            numberOption.Initialize();

            OptionBehaviour = numberOption;

            return numberOption;
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
            opt.Value = newValue;
        }
    }
}

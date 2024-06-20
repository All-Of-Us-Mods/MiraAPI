using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedToggleOption : ModdedOption<bool>
    {
        public ModdedToggleOption(string title, bool defaultValue, System.Type roleType) : base(title, defaultValue, roleType)
        {
        }

        public override OptionBehaviour CreateOption(OptionBehaviour optionBehaviour, Transform container)
        {
            var toggleOption = (ToggleOption)Object.Instantiate(optionBehaviour, container);

            toggleOption.name = Title;
            toggleOption.Title = StringName;
            toggleOption.CheckMark.enabled = Value;
            toggleOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
            toggleOption.Initialize();
            OptionBehaviour = toggleOption;
            return toggleOption;
        }

        public override string GetHudStringText()
        {
            return Title + ": " + (Value ? "On" : "Off");
        }

        public override bool GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
        {
            return optionBehaviour.GetBool();
        }

        public override void OnValueChanged(bool newValue)
        {
            if (OptionBehaviour is null) return;

            var toggleOpt = OptionBehaviour as ToggleOption;
            toggleOpt.CheckMark.enabled = newValue;
        }
    }
}

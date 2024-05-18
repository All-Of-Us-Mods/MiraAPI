using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedToggleOption : ModdedOption<bool>
    {
        public ModdedToggleOption(string title, bool defaultValue, System.Type roleType) : base(title, defaultValue, roleType)
        {
        }

        public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
        {
            var toggleOption = UnityEngine.Object.Instantiate(toggleOpt, container);

            toggleOption.name = Title;
            toggleOption.Title = StringName;
            toggleOption.CheckMark.enabled = Value;
            toggleOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;
            toggleOption.OnEnable();
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

            ToggleOption toggleOpt = OptionBehaviour as ToggleOption;
            toggleOpt.CheckMark.enabled = newValue;
        }
    }
}

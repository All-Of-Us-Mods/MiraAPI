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
            var toggleOption = Object.Instantiate(toggleOpt, Vector3.zero, Quaternion.identity, container);
            var data = ScriptableObject.CreateInstance<CheckboxGameSetting>();
            data.Title = StringName;
            data.Type = global::OptionTypes.Checkbox;

            toggleOption.data = data;

            toggleOption.TitleText.text = Title;
            toggleOption.CheckMark.enabled = Value;
            toggleOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

            OptionBehaviour = toggleOption;

            toggleOption.SetUpFromData(toggleOption.data, 20);

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

            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringName, Value ? "On" : "Off", false);
        }
    }
}

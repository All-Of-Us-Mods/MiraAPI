using System;
using MiraAPI.Networking;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes
{
    public class ModdedToggleOption(string title, bool defaultValue, System.Type roleType) : ModdedOption<bool>(title, defaultValue, roleType)
    {
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

        public override NetData GetNetData()
        {
            return new NetData(Id, BitConverter.GetBytes(Value));
        }

        public override void HandleNetData(byte[] data)
        {
            SetValue(BitConverter.ToBoolean(data));
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
            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringName, newValue ? "On" : "Off", false);
            if (OptionBehaviour is null) return;

            var toggleOpt = OptionBehaviour as ToggleOption;
            if (toggleOpt)
            {
                toggleOpt.CheckMark.enabled = newValue;
            }
        }
    }
}

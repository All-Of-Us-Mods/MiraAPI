using MiraAPI.Networking;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// A modded toggle option.
/// </summary>
public class ModdedToggleOption : ModdedOption<bool>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOption"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="roleType">An optional role type.</param>
    public ModdedToggleOption(string title, bool defaultValue, Type? roleType = null) : base(title, defaultValue, roleType)
    {
        Data = ScriptableObject.CreateInstance<CheckboxGameSetting>();

        var data = (CheckboxGameSetting)Data;
        data.Title = StringName;
        data.Type = global::OptionTypes.Checkbox;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
    {
        var toggleOption = Object.Instantiate(toggleOpt, Vector3.zero, Quaternion.identity, container);

        toggleOption.SetUpFromData(Data, 20);

        toggleOption.Title = StringName;
        toggleOption.TitleText.text = Title;
        toggleOption.CheckMark.enabled = Value;
        toggleOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        OptionBehaviour = toggleOption;

        return toggleOption;
    }

    /// <inheritdoc />
    public override float GetFloatData()
    {
        return Value ? 1 : 0;
    }

    /// <inheritdoc />
    public override NetData GetNetData()
    {
        return new NetData(Id, BitConverter.GetBytes(Value));
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(BitConverter.ToBoolean(data));
    }

    /// <inheritdoc />
    public override bool GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return optionBehaviour.GetBool();
    }

    /// <inheritdoc />
    protected override void OnValueChanged(bool newValue)
    {
        DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringName, newValue ? "On" : "Off", false);
        if (!OptionBehaviour)
        {
            return;
        }

        if (OptionBehaviour is ToggleOption toggleOpt)
        {
            toggleOpt.CheckMark.enabled = newValue;
        }
    }
}

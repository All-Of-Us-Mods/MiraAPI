using System;
using MiraAPI.Networking;
using MiraAPI.Translation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// A modded toggle option.
/// </summary>
public class ModdedToggleOption : ModdedOption<bool>
{
    /// <summary>
    /// Gets or sets the display value when the option is toggled on.
    /// </summary>
    public string OnValue { get; set; }

    /// <summary>
    /// Gets or sets the display value when the option is toggled off.
    /// </summary>
    public string OffValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOption"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="on">What the option displays when toggled on.</param>
    /// <param name="off">What the option displays when toggled off.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedToggleOption(string title, bool defaultValue, string on, string off, bool includeInPreset = true)
        : base(title, defaultValue, includeInPreset)
    {
        OnValue = on;
        OffValue = off;

        var data = ScriptableObject.CreateInstance<CheckboxGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.Checkbox;

        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedToggleOption"/> class.
    /// </summary>
    /// <param name="title">The option title.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedToggleOption(string title, bool defaultValue, bool includeInPreset = true)
        : this(title, defaultValue, "On", "Off", includeInPreset)
    {
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var toggleOption = Object.Instantiate(toggleOpt, Vector3.zero, Quaternion.identity, container);
        toggleOption.name =
            $"{ParentMod!.OptionsTitleText}.ToggleOption.{TranslationController.Instance.GetString(StringName)}";

        toggleOption.SetUpFromData(Data, 20);

        toggleOption.Title = StringName;
        toggleOption.TitleText.text = Title.Translate();
        toggleOption.CheckMark.enabled = Value;
        toggleOption.OnValueChanged = (Action<OptionBehaviour>)ValueChanged;

        OptionBehaviour = toggleOption;

        return toggleOption;
    }

    /// <inheritdoc />
    public override string GetDisplayValue()
    {
        return Value
            ? (OnValue == "On"
                ? TranslationController.Instance.GetString(StringNames.SettingsOn)
                : OnValue.Translate())
            : (OffValue == "Off"
                ? TranslationController.Instance.GetString(StringNames.SettingsOff)
                : OffValue.Translate());
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
        if (OptionBehaviour && OptionBehaviour is ToggleOption toggleOpt)
        {
            toggleOpt.CheckMark.enabled = newValue;
        }
    }
}

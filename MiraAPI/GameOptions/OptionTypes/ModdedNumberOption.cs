using System;
using AmongUs.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// Represents a modded number option.
/// </summary>
public class ModdedNumberOption : ModdedOption<float>
{
    /// <summary>
    /// Gets the minimum value of the option.
    /// </summary>
    public float Min { get; }

    /// <summary>
    /// Gets the maximum value of the option.
    /// </summary>
    public float Max { get; }

    /// <summary>
    /// Gets the increment value of the option.
    /// </summary>
    public float Increment { get; }

    /// <summary>
    /// Gets the suffix type of the option.
    /// </summary>
    public MiraNumberSuffixes SuffixType { get; }

    /// <summary>
    /// Gets a value indicating what zero appears as.
    /// </summary>
    public string ZeroWordValue { get; }

    /// <summary>
    /// Gets a value indicating what a negative one appears as.
    /// </summary>
    public string NegativeWordValue { get; }

    /// <summary>
    /// Gets a value indicating whether zero is infinity.
    /// </summary>
    public bool ZeroInfinity => ZeroWordValue == "∞";

    /// <summary>
    /// Gets a value indicating whether holding shift will divide the increment.
    /// </summary>
    public bool ShiftIncrement { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedNumberOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a float.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    /// <param name="zeroBehavior">Determines what is shown for zero. Options: ∞, #, or a word value (such as Off).</param>
    /// <param name="negativeBehavior">Determines what is shown for negative 1. Options: ∞, #, or a word value (such as Off).</param>
    /// <param name="halfIncrements">Whether increments can be split in half.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedNumberOption(
        string title,
        float defaultValue,
        float min,
        float max,
        float increment,
        string zeroBehavior,
        string negativeBehavior,
        MiraNumberSuffixes suffixType,
        string? formatString = null,
        bool halfIncrements = false,
        bool includeInPreset = true) : base(title, defaultValue, includeInPreset)
    {
        Min = min;
        Max = max;
        Increment = increment;
        SuffixType = suffixType;
        ZeroWordValue = zeroBehavior;
        NegativeWordValue = negativeBehavior;
        ShiftIncrement = halfIncrements;

        Value = Mathf.Clamp(defaultValue, min, max);

        Data = ScriptableObject.CreateInstance<FloatGameSetting>();

        var data = (FloatGameSetting)Data;
        data.Type = global::OptionTypes.Float;
        data.Title = StringName;
        data.Value = Value;
        data.Increment = Increment;
        data.ValidRange = new FloatRange(Min, Max);
        data.FormatString = formatString ??
                            (defaultValue.IsInteger() &&
                             Increment.IsInteger() &&
                             Value.IsInteger() &&
                             Min.IsInteger() &&
                             Max.IsInteger() ? "0" : "0.0");

        data.ZeroIsInfinity = ZeroInfinity;
        data.SuffixType = (NumberSuffixes)SuffixType;
        data.OptionName = FloatOptionNames.Invalid;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedNumberOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a float.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    /// <param name="zeroInfinity">Whether zero is infinity or not.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedNumberOption(
        string title,
        float defaultValue,
        float min,
        float max,
        float increment,
        MiraNumberSuffixes suffixType,
        string? formatString = null,
        bool zeroInfinity = false,
        bool includeInPreset = true) : base(title, defaultValue, includeInPreset)
    {
        Min = min;
        Max = max;
        Increment = increment;
        SuffixType = suffixType;
        ZeroWordValue = zeroInfinity ? "∞" : "#";
        NegativeWordValue = "#";
        ShiftIncrement = Mathf.Approximately(increment, 1f);

        Value = Mathf.Clamp(defaultValue, min, max);

        Data = ScriptableObject.CreateInstance<FloatGameSetting>();

        var data = (FloatGameSetting)Data;
        data.Type = global::OptionTypes.Float;
        data.Title = StringName;
        data.Value = Value;
        data.Increment = Increment;
        data.ValidRange = new FloatRange(Min, Max);
        data.FormatString = formatString ??
                            (defaultValue.IsInteger() &&
                             Increment.IsInteger() &&
                             Value.IsInteger() &&
                             Min.IsInteger() &&
                             Max.IsInteger() ? "0" : "0.0");

        data.ZeroIsInfinity = ZeroInfinity;
        data.SuffixType = (NumberSuffixes)SuffixType;
        data.OptionName = FloatOptionNames.Invalid;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(
        ToggleOption toggleOpt,
        NumberOption numberOpt,
        StringOption stringOpt,
        PlayerOption playerOpt,
        Transform container)
    {
        var numberOption = Object.Instantiate(numberOpt, Vector3.zero, Quaternion.identity, container);
        numberOption.name =
            $"{ParentMod!.OptionsTitleText}.NumberOption.{TranslationController.Instance.GetString(StringName)}";
        var optionComponent = numberOption.gameObject.AddComponent<MiraNumberOptionComponent>();
        optionComponent.NumberOption = this;
        optionComponent.DefaultIncrement = Increment;
        optionComponent.ShiftIncrementToggle = ShiftIncrement;
        optionComponent.NegativeValue = NegativeWordValue;
        optionComponent.ZeroValue = ZeroWordValue;

        numberOption.SetUpFromData(Data, 20);
        numberOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        numberOption.Title = StringName;
        numberOption.Value = Value;
        numberOption.Increment = Increment;
        numberOption.ValidRange = new FloatRange(Min, Max);
        numberOption.FormatString = (Data as FloatGameSetting)?.FormatString ?? "0.0";
        numberOption.ZeroIsInfinity = ZeroInfinity;
        numberOption.SuffixType = (NumberSuffixes)SuffixType;
        numberOption.floatOptionName = FloatOptionNames.Invalid;

        OptionBehaviour = numberOption;

        return numberOption;
    }

    /// <inheritdoc />
    public override float GetFloatData()
    {
        return Value;
    }

    /// <inheritdoc />
    public override NetData GetNetData()
    {
        return new NetData(Id, BitConverter.GetBytes(Value));
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(BitConverter.ToSingle(data));
    }

    /// <inheritdoc />
    public override float GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return Mathf.Clamp(optionBehaviour.GetFloat(), Min, Max);
    }

    /// <inheritdoc />
    protected override void OnValueChanged(float newValue)
    {
        Value = Mathf.Clamp(newValue, Min, Max);
        ModdedOptionsManager.AddSettingsChangeMessage(
            HudManager.Instance.Notifier,
            StringName,
            Data.GetValueString(newValue),
            Configuration.PopUpTextColor,
            Configuration.PopUpIconTmp,
            false);

        if (OptionBehaviour is NumberOption opt)
        {
            opt.Value = Value;
        }
    }

    /// <summary>
    /// Implicitly converts the option to an <see langword="int"/>.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <returns>Integer value.</returns>
    public static implicit operator int(ModdedNumberOption option)
    {
        return (int)option.Value;
    }
}

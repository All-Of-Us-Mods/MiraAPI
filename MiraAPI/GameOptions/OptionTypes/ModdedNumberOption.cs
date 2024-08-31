using AmongUs.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using System;
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
    /// Gets a value indicating whether zero is infinity.
    /// </summary>
    public bool ZeroInfinity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedNumberOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a float.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="zeroInfinity">Whether zero is infinity or not.</param>
    /// <param name="roleType">An optional role type.</param>
    public ModdedNumberOption(
        string title,
        float defaultValue,
        float min,
        float max,
        float increment,
        MiraNumberSuffixes suffixType,
        bool zeroInfinity = false,
        Type? roleType = null) : base(title, defaultValue, roleType)
    {
        Min = min;
        Max = max;
        Increment = increment;
        SuffixType = suffixType;
        ZeroInfinity = zeroInfinity;

        Value = Mathf.Clamp(defaultValue, min, max);

        Data = ScriptableObject.CreateInstance<FloatGameSetting>();

        var data = (FloatGameSetting)Data;
        data.Type = global::OptionTypes.Float;
        data.Title = StringName;
        data.Value = Value;
        data.Increment = Increment;
        data.ValidRange = new FloatRange(Min, Max);
        data.FormatString = Increment % 1 == 0 && Value % 1 == 0 && Min % 1 == 0 && Max % 1 == 0 ? "0" : "0.0"; // not sure if the == checks will work since this is floats but we'll find out
        data.ZeroIsInfinity = ZeroInfinity;
        data.SuffixType = (NumberSuffixes)SuffixType;
        data.OptionName = FloatOptionNames.Invalid;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(
        ToggleOption toggleOpt,
        NumberOption numberOpt,
        StringOption stringOpt,
        Transform container)
    {
        var numberOption = Object.Instantiate(numberOpt, Vector3.zero, Quaternion.identity, container);

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
        DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(
            StringName,
            Data?.GetValueString(Value),
            false);

        if (OptionBehaviour is NumberOption opt)
        {
            opt.Value = Value;
        }
    }
}

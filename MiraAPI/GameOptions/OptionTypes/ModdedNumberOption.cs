using AmongUs.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

public class ModdedNumberOption : ModdedOption<float>
{
    public float Min { get; }

    public float Max { get; }

    public float Increment { get; }

    public MiraNumberSuffixes SuffixType { get; }

    public bool ZeroInfinity { get; }

    public ModdedNumberOption(string title, float defaultValue, float min, float max, float increment, MiraNumberSuffixes suffixType, bool zeroInfinity = false, Type roleType = null) : base(title, defaultValue, roleType)
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
        data.FormatString = Increment % 1 == 0 && Value % 1 == 0 && Min % 1 == 0 && Max % 1 == 0 ? "0" : "0.0";
        data.ZeroIsInfinity = ZeroInfinity;
        data.SuffixType = (NumberSuffixes)SuffixType;
        data.OptionName = FloatOptionNames.Invalid;
    }

    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
    {
        var numberOption = Object.Instantiate(numberOpt, Vector3.zero, Quaternion.identity, container);

        numberOption.SetUpFromData(Data, 20);
        numberOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        numberOption.Title = StringName;
        numberOption.Value = Value;
        numberOption.Increment = Increment;
        numberOption.ValidRange = new FloatRange(Min, Max);
        numberOption.FormatString = ((FloatGameSetting)Data).FormatString;
        numberOption.ZeroIsInfinity = ZeroInfinity;
        numberOption.SuffixType = (NumberSuffixes)SuffixType;
        numberOption.floatOptionName = FloatOptionNames.Invalid;


        OptionBehaviour = numberOption;

        return numberOption;
    }

    public override float GetFloatData()
    {
        return Value;
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
        DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringName, Data.GetValueString(Value), false);

        if (!OptionBehaviour)
        {
            return;
        }

        var opt = OptionBehaviour as NumberOption;
        if (opt)
        {
            opt.Value = newValue;
        }
    }
}
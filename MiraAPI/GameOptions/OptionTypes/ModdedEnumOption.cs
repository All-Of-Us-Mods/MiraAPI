using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Networking;
using Reactor.Localization.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// An option for selecting an enum value.
/// </summary>
public class ModdedEnumOption : ModdedOption<int>
{
    /// <summary>
    /// Gets the string values of the enum.
    /// </summary>
    public string[]? Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an int.</param>
    /// <param name="enumType">The Enum type.</param>
    /// <param name="values">An option list of string values to use in place of the enum name.</param>
    public ModdedEnumOption(string title, int defaultValue, Type enumType, string[]? values = null) : base(title, defaultValue)
    {
        Values = values ?? Enum.GetNames(enumType);
        Data = ScriptableObject.CreateInstance<StringGameSetting>();
        var data = (StringGameSetting)Data;

        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = values is null ?
            Enum.GetNames(enumType).Select(CustomStringName.CreateAndRegister).ToArray()
            : values.Select(CustomStringName.CreateAndRegister).ToArray();

        data.Index = Value;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        // SetUpFromData method doesnt work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Values = (Data as StringGameSetting)?.Values ?? new Il2CppStructArray<StringNames>(0);
        stringOption.Value = Value;
        stringOption.AdjustButtonsActiveState();

        OptionBehaviour = stringOption;

        return stringOption;
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
        SetValue(BitConverter.ToInt32(data));
    }

    /// <inheritdoc />
    public override int GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return optionBehaviour.GetInt();
    }

    /// <inheritdoc />
    protected override void OnValueChanged(int newValue)
    {
        HudManager.Instance.Notifier.AddSettingsChangeMessage(StringName, Data?.GetValueString(newValue), false);
        if (!OptionBehaviour)
        {
            return;
        }

        if (OptionBehaviour is StringOption opt)
        {
            opt.Value = newValue;
        }
    }
}

/// <summary>
/// An option for selecting an enum value.
/// </summary>
/// <typeparam name="T">The enum type.</typeparam>
public class ModdedEnumOption<T> : ModdedOption<T> where T : Enum
{
    /// <summary>
    /// Gets the string values of the enum.
    /// </summary>
    public string[]? Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption{T}"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an int.</param>
    /// <param name="values">An option list of string values to use in place of the enum name.</param>
    public ModdedEnumOption(string title, T defaultValue, string[]? values = null) : base(title, defaultValue)
    {
        Values = values ?? Enum.GetNames(typeof(T));
        Data = ScriptableObject.CreateInstance<StringGameSetting>();
        var data = (StringGameSetting)Data;

        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = values is null ?
            Enum.GetNames(typeof(T)).Select(CustomStringName.CreateAndRegister).ToArray()
            : values.Select(CustomStringName.CreateAndRegister).ToArray();

        data.Index = Convert.ToInt32(Value);
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        // SetUpFromData method doesnt work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Values = (Data as StringGameSetting)?.Values ?? new Il2CppStructArray<StringNames>(0);
        stringOption.Value = Convert.ToInt32(Value, NumberFormatInfo.InvariantInfo);

        OptionBehaviour = stringOption;

        return stringOption;
    }

    /// <inheritdoc />
    public override float GetFloatData()
    {
        return Convert.ToSingle(Value, NumberFormatInfo.InvariantInfo);
    }

    /// <inheritdoc />
    public override NetData GetNetData()
    {
        return new NetData(Id, Encoding.Unicode.GetBytes(Convert.ToString(Value, NumberFormatInfo.InvariantInfo)!));
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue((T)Enum.Parse(typeof(T), Encoding.Unicode.GetString(data)));
    }

    /// <inheritdoc />
    public override T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return (T)Enum.Parse(typeof(T), optionBehaviour.GetInt().ToString(NumberFormatInfo.InvariantInfo));
    }

    /// <inheritdoc />
    protected override void OnValueChanged(T newValue)
    {
        HudManager.Instance.Notifier.AddSettingsChangeMessage(StringName, Data.GetValueString(Convert.ToInt32(newValue, NumberFormatInfo.InvariantInfo)), false);
        if (!OptionBehaviour)
        {
            return;
        }

        if (OptionBehaviour is StringOption opt)
        {
            opt.Value = Convert.ToInt32(newValue, NumberFormatInfo.InvariantInfo);
        }
    }
}

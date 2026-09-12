using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Networking;
using MiraAPI.Translation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// An option for selecting an <see langword="enum"/> value.
/// </summary>
public class ModdedEnumOption : ModdedOption<int>
{
    /// <summary>
    /// Gets the string values of the enum.
    /// </summary>
    public string[] Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an int.</param>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string title, int defaultValue, Type enumType, string[]? values = null, bool includeInPreset = true) : base(title, defaultValue, includeInPreset)
    {
        Values = values ?? Enum.GetNames(enumType);

        var data = ScriptableObject.CreateInstance<StringGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = Values.Select(MiraLocaleManager.GetOrCreateLocaleString).ToArray();
        data.Index = Value;

        Data = data;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);
        stringOption.name =
            $"{ParentMod!.OptionsTitleText}.EnumOption.{TranslationController.Instance.GetString(StringName)}";

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        // SetUpFromData method doesn't work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Values = (Data as StringGameSetting)?.Values ?? new Il2CppStructArray<StringNames>(0);
        stringOption.Value = Value;

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
        ModdedOptionsManager.AddSettingsChangeMessage(
            HudManager.Instance.Notifier,
            StringName,
            Data.GetValueString(newValue),
            Configuration.PopUpTextColor,
            Configuration.PopUpIconTmp,
            false);
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
/// An option for selecting an <see langword="enum"/> value.
/// </summary>
/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
public class ModdedEnumOption<T> : ModdedOption<T> where T : Enum
{
    /// <summary>
    /// Gets the string values of the <see langword="enum"/>.
    /// </summary>
    public string[] Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption{T}"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an <see langword="int"/>.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string title, T defaultValue, string[]? values = null, bool includeInPreset = true) : base(title, defaultValue, includeInPreset)
    {
        Values = values ?? Enum.GetNames(typeof(T));

        var data = ScriptableObject.CreateInstance<StringGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = Values.Select(MiraLocaleManager.GetOrCreateLocaleString).ToArray();
        data.Index = Convert.ToInt32(Value, NumberFormatInfo.InvariantInfo);

        Data = data;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);
        stringOption.name =
            $"{ParentMod!.OptionsTitleText}.EnumOption.{TranslationController.Instance.GetString(StringName)}";

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        // SetUpFromData method doesn't work correctly so we must set the values manually
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
        ModdedOptionsManager.AddSettingsChangeMessage(
            HudManager.Instance.Notifier,
            StringName,
            Data.GetValueString(Convert.ToInt32(newValue, NumberFormatInfo.InvariantInfo)),
            Configuration.PopUpTextColor,
            Configuration.PopUpIconTmp,
            false);
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

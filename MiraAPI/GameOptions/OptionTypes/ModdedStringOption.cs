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
/// An option for selecting an enum value.
/// </summary>
public class ModdedStringOption : ModdedOption<string>
{
    /// <summary>
    /// Gets or sets the string values of the option.
    /// </summary>
    public string[] Values { get; set; }

    /// <summary>
    /// Gets or sets the String Name values of the option.
    /// </summary>
    public StringNames[] StringNameValues { get; set; }

    /// <summary>
    /// Gets the string's index.
    /// </summary>
    /// <param name="value">The option's string value.</param>
    /// <returns>The option's value in the index.</returns>
    public int GetIndex(string value, int location)
    {
        if (Values.Contains(value))
        {
            return Values.ToList().IndexOf(value);
        }
        Info($"Cannot find a valid index for {value} in {location}. Returning 0 instead. Values: {string.Join(", ", Values)}");
        return 0;
    }

    /// <summary>
    /// Adds a value to the option.
    /// </summary>
    /// <param name="value">The option's string value.</param>
    /// <param name="index">The additional value's index on the list.</param>
    public void RegisterExtraValue(string value, int index = -1)
    {
        if (!Values.Contains(value))
        {
            if (index <= -1)
                index = Values.Length;

            var newVals = Values.ToList();
            newVals.Insert(index, value);
            Values = newVals.ToArray();
            var newStrVals = StringNameValues.ToList();
            newStrVals.Insert(index, MiraLocaleManager.GetOrCreateLocaleString(value));
            StringNameValues = newStrVals.ToArray();
        }

        if (Data != null)
        {
            var data = (StringGameSetting)Data;
            data.Values = StringNameValues;

            data.Index = GetIndex(Value, 1);
            if (OptionBehaviour != null)
            {
                var strOpt = OptionBehaviour.TryCast<StringOption>();
                if (strOpt != null)
                {
                    strOpt.Values = StringNameValues;
                    strOpt.Value = GetIndex(Value, 2);
                    Warning("StringOption is valid");
                }
                else
                {
                    Error("StringOption is null.");
                }
                Warning("OptionBehaviour is valid");
            }
            else
            {
                Error("OptionBehaviour is null.");
            }
            Warning("Data is valid");
        }
        else
        {
            Error("Data is null.");
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedStringOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a string.</param>
    /// <param name="values">An option list of string values to use.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedStringOption(string title, string defaultValue, string[] values, bool includeInPreset=true) : base(title, defaultValue, includeInPreset)
    {
        Values = values;
        Data = ScriptableObject.CreateInstance<StringGameSetting>();
        var data = (StringGameSetting)Data;

        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        StringNameValues = values.Select(MiraLocaleManager.GetOrCreateLocaleString).ToArray();
        data.Values = StringNameValues;

        data.Index = GetIndex(Value, 3);
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);
        stringOption.name =
            $"{ParentMod!.OptionsTitleText}.StringOption.{TranslationController.Instance.GetString(StringName)}";

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Il2CppSystem.Action<OptionBehaviour>)ValueChanged;

        // SetUpFromData method doesnt work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Values = (Data as StringGameSetting)?.Values ?? new Il2CppStructArray<StringNames>(0);
        stringOption.Value = GetIndex(Value, 4);

        OptionBehaviour = stringOption;
        ModdedOptionsManager.CreatedStringOptions.TryAdd(stringOption, this);

        return stringOption;
    }

    /// <inheritdoc />
    public override float GetFloatData()
    {
        return Array.IndexOf(Values, Value);
    }

    /// <inheritdoc />
    public override NetData GetNetData()
    {
        return new NetData(Id, Encoding.Unicode.GetBytes(Convert.ToString(Value, NumberFormatInfo.InvariantInfo)));
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(Encoding.Unicode.GetString(data));
    }

    /// <inheritdoc />
    public override string GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return Values[optionBehaviour.GetInt()];
    }

    /// <inheritdoc />
    protected override void OnValueChanged(string newValue)
    {
        HudManager.Instance.Notifier.AddSettingsChangeMessage(StringName, Data.GetValueString(GetIndex(newValue, 5)), false);
        if (!OptionBehaviour)
        {
            return;
        }

        if (OptionBehaviour is StringOption opt)
        {
            opt.Value = GetIndex(newValue, 5);
        }
    }
}

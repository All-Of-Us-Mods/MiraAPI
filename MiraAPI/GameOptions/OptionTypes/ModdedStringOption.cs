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
    /// Initializes a new instance of the <see cref="ModdedStringOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a string.</param>
    /// <param name="values">An option list of string values to use.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedStringOption(string title, string defaultValue, string[] values, bool includeInPreset = true)
        : base(title, defaultValue, includeInPreset)
    {
        Values = values;
        StringNameValues = [.. values.Select(MiraLocaleManager.GetOrCreateLocaleString)];

        var data = ScriptableObject.CreateInstance<StringGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = StringNameValues;
        data.Index = GetIndex(Value);

        Data = data;
    }

    /// <summary>
    /// Gets the string's index.
    /// </summary>
    /// <param name="value">The option's string value.</param>
    /// <returns>The option's value in the index.</returns>
    public int GetIndex(string value)
    {
        if (Values.Contains(value))
        {
            return Array.IndexOf(Values, value);
        }

        Info($"Cannot find a valid index for {value}. Returning -1 instead. Values: {string.Join(", ", Values)}");
        return -1;
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
            Values = [.. newVals];
            var newStrVals = StringNameValues.ToList();
            newStrVals.Insert(index, MiraLocaleManager.GetOrCreateLocaleString(value));
            StringNameValues = [.. newStrVals];
        }

        if (Data == null)
        {
            Error("Data is null.");
            return;
        }

        var data = (StringGameSetting)Data;
        data.Values = StringNameValues;
        data.Index = GetIndex(Value);

        Warning("Data is valid");

        if (OptionBehaviour == null)
        {
            Error("OptionBehaviour is null.");
            return;
        }

        var strOpt = OptionBehaviour.TryCast<StringOption>();

        Warning("OptionBehaviour is valid");

        if (strOpt == null)
        {
            Error("StringOption is null.");
            return;
        }

        strOpt.Values = StringNameValues;
        strOpt.Value = GetIndex(Value);
        Warning("StringOption is valid");
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);
        stringOption.name =
            $"{ParentMod!.OptionsTitleText}.StringOption.{TranslationController.Instance.GetString(StringName)}";

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Action<OptionBehaviour>)ValueChanged;

        // Changing the button text to make them more distinct from number options
        stringOption.MinusBtn.ChangeButtonText("<");
        stringOption.PlusBtn.ChangeButtonText(">");

        // SetUpFromData method doesn't work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Values = (Data as StringGameSetting)?.Values ?? new Il2CppStructArray<StringNames>(0);
        stringOption.Value = GetIndex(Value);

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
        if (OptionBehaviour && OptionBehaviour is StringOption opt)
        {
            opt.Value = GetIndex(newValue);
        }
    }
}

using System;
using AmongUs.GameOptions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// A base class for modded options that utilize a string-based interface.
/// </summary>
/// <typeparam name="T">The underlying option type.</typeparam>
/// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
/// <param name="title">The title of the option.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
public abstract class ModdedBaseStringOption<T>(string groupPrefix, string title, T defaultValue, bool includeInPreset = true)
    : ModdedOption<T>(groupPrefix, title, defaultValue, includeInPreset)
{
    /// <summary>
    /// Gets or sets the string values of the option.
    /// </summary>
    public string[] Values { get; protected set; } = [];

    /// <summary>
    /// Gets or sets the underlying values of the option.
    /// </summary>
    public T[] UnderlyingValues { get; protected set; } = [];

    /// <summary>
    /// Gets or sets the String Name values of the option.
    /// </summary>
    public StringNames[] StringNameValues { get; protected set; } = [];

    /// <summary>
    /// Gets the option type name used for naming the generated GameObject.
    /// </summary>
    protected abstract string OptionTypeName { get; }

    /// <summary>
    /// Initializes the common StringGameSetting data for this option.
    /// </summary>
    /// <param name="defaultIndex">The default index to assign.</param>
    protected void InitializeData(int defaultIndex)
    {
        var data = ScriptableObject.CreateInstance<StringGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = StringNameValues;
        data.Index = defaultIndex;

        Data = data;
    }

    /// <summary>
    /// Gets the index of a specific value.
    /// </summary>
    /// <param name="value">The option's value.</param>
    /// <returns>The index of the value in the underlying array.</returns>
    public int GetIndex(T value)
    {
        var index = Array.IndexOf(UnderlyingValues, value);

        if (index == -1)
            Info($"Cannot find a valid index for {value}. Returning -1 instead. Values: {string.Join(", ", Values)}");

        return index;
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);
        stringOption.name = $"{ParentMod!.OptionsTitleText}.{OptionTypeName}.{Title}";

        stringOption.SetUpFromData(Data, 20);

        // SetUpFromData method doesn't work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Value = GetIndex(Value);
        stringOption.Values = StringNameValues;
        stringOption.stringOptionName = Int32OptionNames.Invalid;
        stringOption.OnValueChanged = (Action<OptionBehaviour>)ValueChanged;

        // Changing the button text to make them more distinct from number options
        stringOption.MinusBtn.ChangeButtonText("<");
        stringOption.PlusBtn.ChangeButtonText(">");

        OptionBehaviour = stringOption;

        return stringOption;
    }

    /// <inheritdoc />
    protected override void OnValueChanged(T newValue)
    {
        if (OptionBehaviour && OptionBehaviour is StringOption opt)
        {
            opt.Value = GetIndex(newValue);
        }
    }

    /// <inheritdoc />
    public override string GetDisplayValue()
    {
        var index = GetIndex(Value);
        return (uint)index < (uint)StringNameValues.Length
            ? TranslationController.Instance.GetString(StringNameValues[index])
            : Value!.ToString()!;
    }

    /// <inheritdoc />
    public override float GetFloatData()
    {
        return GetIndex(Value);
    }

    /// <inheritdoc />
    public override T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        var index = optionBehaviour.GetInt();

        if ((uint)index < (uint)UnderlyingValues.Length)
            return UnderlyingValues[index];

        return UnderlyingValues.Length > 0 ? UnderlyingValues[0] : default!;
    }
}

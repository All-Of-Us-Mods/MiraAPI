using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using MiraAPI.Networking;
using MiraAPI.Translation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// An option for selecting an <see langword="enum"/> value.
/// </summary>
[Obsolete("Use ModdedEnumOption<T> instead. This will be removed in a future version.")]
public class ModdedEnumOption : ModdedOption<int>
{
    private readonly int[] underlyingValues;

    /// <summary>
    /// Gets the string values of the enum.
    /// </summary>
    public string[] Values { get; }

    /// <summary>
    /// Gets the String Name values of the option.
    /// </summary>
    public StringNames[] StringNameValues { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an int.</param>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string groupPrefix, string title, int defaultValue, Type enumType, string idPrefix, string[]? values = null, bool includeInPreset = true)
        : base(groupPrefix, title, defaultValue, includeInPreset)
    {
        var rawValues = Enum.GetValues(enumType);
        underlyingValues = new int[rawValues.Length];

        for (var i = 0; i < rawValues.Length; i++)
            underlyingValues[i] = Convert.ToInt32(rawValues.GetValue(i), NumberFormatInfo.InvariantInfo);

        Values = values ?? Enum.GetNames(enumType);

        StringNameValues = [.. Values.Select(x => MiraLocaleManager.GetOrCreateLocaleString($"{idPrefix}{x}"))];

        var data = ScriptableObject.CreateInstance<StringGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = StringNameValues;
        data.Index = Array.IndexOf(underlyingValues, defaultValue);

        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an int.</param>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string groupPrefix, string title, int defaultValue, Type enumType, string[]? values = null, bool includeInPreset = true)
        : this(groupPrefix, title, defaultValue, enumType, string.Empty, values, includeInPreset)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an int.</param>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string title, int defaultValue, Type enumType, string[]? values = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, enumType, values, includeInPreset)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an int.</param>
    /// <param name="enumType">The <see cref="Enum"/> type.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string title, int defaultValue, Type enumType, string idPrefix, string[]? values = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, enumType, idPrefix, values, includeInPreset)
    {
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);
        stringOption.name = $"{ParentMod!.OptionsTitleText}.EnumOption.{Title}";

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Action<OptionBehaviour>)ValueChanged;

        // Changing the button text to make them more distinct from number options
        stringOption.MinusBtn.ChangeButtonText("<");
        stringOption.PlusBtn.ChangeButtonText(">");

        // SetUpFromData method doesn't work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Values = StringNameValues;
        stringOption.Value = Array.IndexOf(underlyingValues, Value);

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
        var index = optionBehaviour.GetInt();

        if (index >= 0 && index < underlyingValues.Length)
            return underlyingValues[index];

        return underlyingValues.Length > 0 ? underlyingValues[0] : 0;
    }

    /// <inheritdoc />
    protected override void OnValueChanged(int newValue)
    {
        if (OptionBehaviour && OptionBehaviour is StringOption opt)
        {
            opt.Value = Array.IndexOf(underlyingValues, newValue);
        }
    }
}

/// <summary>
/// An option for selecting an <see langword="enum"/> value.
/// </summary>
/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
public class ModdedEnumOption<T> : ModdedOption<T>
    where T : struct, Enum
{
    private readonly T[] underlyingValues;

    /// <summary>
    /// Gets the string values of the <see langword="enum"/>.
    /// </summary>
    public string[] Values { get; }

    /// <summary>
    /// Gets the String Name values of the option.
    /// </summary>
    public StringNames[] StringNameValues { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption{T}"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an <see langword="int"/>.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, string idPrefix, string[]? values = null, bool includeInPreset = true)
        : base(groupPrefix, title, defaultValue, includeInPreset)
    {
        underlyingValues = Enum.GetValues<T>();

        Values = values ?? Enum.GetNames<T>();
        StringNameValues = [.. Values.Select(x => MiraLocaleManager.GetOrCreateLocaleString($"{idPrefix}{x}"))];

        var data = ScriptableObject.CreateInstance<StringGameSetting>();
        data.Title = StringName;
        data.Type = global::OptionTypes.String;
        data.Values = StringNameValues;
        data.Index = Array.IndexOf(underlyingValues, defaultValue);

        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption{T}"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an <see langword="int"/>.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, string[]? values = null, bool includeInPreset = true)
        : this(groupPrefix, title, defaultValue, string.Empty, values, includeInPreset)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption{T}"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an <see langword="int"/>.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string title, T defaultValue, string[]? values = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, values, includeInPreset)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption{T}"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an <see langword="int"/>.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    /// <param name="values">An option list of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string title, T defaultValue, string idPrefix, string[]? values = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, idPrefix, values, includeInPreset)
    {
    }

    /// <inheritdoc />
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var stringOption = Object.Instantiate(stringOpt, container);
        stringOption.name = $"{ParentMod!.OptionsTitleText}.EnumOption.{Title}";

        stringOption.SetUpFromData(Data, 20);
        stringOption.OnValueChanged = (Action<OptionBehaviour>)ValueChanged;

        // Changing the button text to make them more distinct from number options
        stringOption.MinusBtn.ChangeButtonText("<");
        stringOption.PlusBtn.ChangeButtonText(">");

        // SetUpFromData method doesn't work correctly so we must set the values manually
        stringOption.Title = StringName;
        stringOption.Values = StringNameValues;
        stringOption.Value = Array.IndexOf(underlyingValues, Value);

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
        var buffer = new byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref buffer[0], Value);
        return new NetData(Id, buffer);
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(Unsafe.ReadUnaligned<T>(ref data[0]));
    }

    /// <inheritdoc />
    public override T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return Enum.Parse<T>(optionBehaviour.GetInt().ToString(NumberFormatInfo.InvariantInfo));
    }

    /// <inheritdoc />
    protected override void OnValueChanged(T newValue)
    {
        if (OptionBehaviour && OptionBehaviour is StringOption opt)
        {
            opt.Value = Convert.ToInt32(newValue, NumberFormatInfo.InvariantInfo);
        }
    }
}

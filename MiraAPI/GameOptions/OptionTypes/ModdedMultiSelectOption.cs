using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using MiraAPI.GameOptions.Components;
using MiraAPI.Networking;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes;

// TODO: Add documentation once completed
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public interface IMultiSelectOption
{
    int Value { get; }
}

public class ModdedMultiSelectOption<T> : ModdedOption<T>, IMultiSelectOption
    where T : struct, Enum
{
    /// <summary>
    /// Gets an array of int values where only a bit is turned on to denote each flag contained within <typeparamref name="T"/>.
    /// </summary>
    public int[] Flags { get; }

    /// <summary>
    /// Gets the string values of the option.
    /// </summary>
    public string[] Values { get; } = [];

    /// <summary>
    /// Gets the String Name values of the option.
    /// </summary>
    public StringNames[] StringNameValues { get; } = [];

    public StringNames NoneName { get; }

    public StringNames AllName { get; }

    public int All { get; }

    int IMultiSelectOption.Value => Convert.ToInt32(Value, NumberFormatInfo.InvariantInfo);

    public ModdedMultiSelectOption(string groupPrefix, string title, T defaultValue, string idPrefix, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : base(groupPrefix, title, defaultValue, includeInPreset)
    {
        var enumType = typeof(T);

        if (!enumType.IsDefined<FlagsAttribute>(false))
            throw new ArgumentException($"{enumType.Name} does not have [Flags], and therefore cannot be used for multi-select options.");

        var targetValues = allowedValues ?? Enum.GetValues<T>();

        var tuples = targetValues
            .Select(x => (Val: x, Bit: Convert.ToInt32(x, NumberFormatInfo.InvariantInfo)))
            .Where(val => val.Bit > 0 && (val.Bit & (val.Bit - 1)) == 0)
            .DistinctBy(x => x.Bit)
            .ToArray();

        if (values != null && values.Length != tuples.Length)
        {
            throw new ArgumentException($"The number of custom values ({values.Length}) must exactly match the number of allowed single-bit flags ({tuples.Length}).", nameof(values));
        }

        var all = 0;

        foreach (var (_, bit) in tuples)
        {
            all |= bit;
        }

        All = all;

        var defaultBits = Convert.ToInt32(defaultValue, NumberFormatInfo.InvariantInfo);

        if ((defaultBits & ~All) != 0)
        {
            throw new ArgumentException($"The defaultValue contains flags that are not present in the allowed subset.", nameof(defaultValue));
        }

        idPrefix = idPrefix.Replace("%type%", enumType.Name);

        Flags = [.. tuples.Select(x => x.Bit)];
        Values = values ?? [.. tuples.Select(x => x.Val.ToString())];
        AllName = MiraLocaleManager.GetOrCreateLocaleString(idPrefix + (allId ?? "All"));
        NoneName = MiraLocaleManager.GetOrCreateLocaleString(idPrefix + (noneId ?? "None"));
        StringNameValues = [.. Values.Select(x => MiraLocaleManager.GetOrCreateLocaleString(idPrefix + x))];

        var data = ScriptableObject.CreateInstance<FlagsGameSetting>();
        data.Type = global::OptionTypes.MultipleChoice; // How nice of Innersloth to provide us with a pre-defined enum value
        data.Value = defaultBits;
        data.All = All;
        data.Flags = Flags;
        data.Values = StringNameValues;

        Data = data;
    }

    public ModdedMultiSelectOption(string groupPrefix, string title, T defaultValue, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : this(groupPrefix, title, defaultValue, string.Empty, allowedValues, values, noneId, allId, includeInPreset)
    {
    }

    public ModdedMultiSelectOption(string title, T defaultValue, string idPrefix, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, idPrefix, allowedValues, values, noneId, allId, includeInPreset)
    {
    }

    public ModdedMultiSelectOption(string title, T defaultValue, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, string.Empty, allowedValues, values, noneId, allId, includeInPreset)
    {
    }

    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        throw new NotImplementedException();
    }

    public override float GetFloatData()
    {
        return Convert.ToInt32(_value, NumberFormatInfo.InvariantInfo);
    }

    public override NetData GetNetData()
    {
        var buffer = new byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref buffer[0], _value);
        return new(Id, buffer);
    }

    public override void HandleNetData(byte[] data)
    {
        SetValue(Unsafe.ReadUnaligned<T>(ref data[0]));
    }

    public override T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        throw new NotImplementedException();
    }

    protected override void OnValueChanged(T newValue)
    {
        throw new NotImplementedException();
    }
}

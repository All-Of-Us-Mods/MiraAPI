using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using MiraAPI.GameOptions.Components;
using MiraAPI.Networking;
using MiraAPI.Patches.Options;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// An interface used to mark an option class as a flags option.
/// </summary>
public interface IFlagsOption
{
    /// <summary>
    /// Gets the combined bits value of the option's selected flags.
    /// </summary>
    int Value { get; }

    /// <inheritdoc cref="IModdedOption.GetDisplayValue"/>
    string GetDisplayValue();
}

/// <summary>
/// An option for selecting multiple <typeparamref name="T"/> values.
/// </summary>
/// <typeparam name="T">The type of the flag enum used as the backing value.</typeparam>
public class ModdedFlagsOption<T> : ModdedOption<T>, IFlagsOption
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

    /// <summary>
    /// Gets the <see cref="StringNames"/> ID of the translation that corresponds to when no options are selected.
    /// </summary>
    public StringNames NoneName { get; }

    /// <summary>
    /// Gets the <see cref="StringNames"/> ID of the translation that corresponds to when all options are selected.
    /// </summary>
    public StringNames AllName { get; }

    /// <summary>
    /// Gets the "maximum" value of the flag option, which is where all defined flags are turned on.
    /// </summary>
    public int All { get; }

    /// <summary>
    /// Gets a (possibly) filtered array of strongly typed values that are allowed to be used by the option.
    /// </summary>
    public T[] AllowedValues { get; }

    /// <inheritdoc/>
    int IFlagsOption.Value => value;

    private readonly string allTranslationId;
    private readonly string noneTranslationId;

    private int value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedFlagsOption{T}"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an <see langword="int"/>.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    /// <param name="allowedValues">An option array of enum values to filter down what values can actually be used.</param>
    /// <param name="values">An optional array of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="noneId">The core part of the translation ID corresponding to when no options are selected.</param>
    /// <param name="allId">The core part of the translation ID corresponding to when all options are selected.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedFlagsOption(string groupPrefix, string title, T defaultValue, string idPrefix, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : base(groupPrefix, title, defaultValue, includeInPreset)
    {
        var enumType = typeof(T);

        if (!enumType.IsDefined<FlagsAttribute>(false))
            throw new ArgumentException($"{enumType.Name} does not have [Flags], and therefore cannot be used for multi-select options.");

        allowedValues ??= Enum.GetValues<T>();
        allowedValues = [.. allowedValues.Distinct()];

        var tuples = allowedValues
            .Select(x => (Val: x, Bit: Convert.ToInt32(x, NumberFormatInfo.InvariantInfo)))
            .Where(val => val.Bit > 0 && (val.Bit & (val.Bit - 1)) == 0)
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

        var defaultBits = Convert.ToInt32(defaultValue, NumberFormatInfo.InvariantInfo);

        if ((defaultBits & ~all) != 0)
        {
            throw new ArgumentException($"The defaultValue contains flags that are not present in the allowed subset.", nameof(defaultValue));
        }

        idPrefix = idPrefix.Replace("%type%", enumType.Name);

        value = defaultBits;
        allTranslationId = idPrefix + (allId ?? "All");
        noneTranslationId = idPrefix + (noneId ?? "None");

        All = all;
        Flags = [.. tuples.Select(x => x.Bit)];
        AllowedValues = [.. tuples.Select(x => x.Val)];
        AllName = MiraLocaleManager.GetOrCreateLocaleString(allTranslationId);
        NoneName = MiraLocaleManager.GetOrCreateLocaleString(noneTranslationId);
        Values = [.. (values ?? AllowedValues.Cast<object>()).Select(x => idPrefix + x)];
        StringNameValues = [.. Values.Select(MiraLocaleManager.GetOrCreateLocaleString)];

        var data = ScriptableObject.CreateInstance<FlagsGameSetting>();
        data.Type = global::OptionTypes.MultipleChoice; // How nice of Innersloth to provide us with a pre-defined enum value
        data.Value = defaultBits;
        data.All = All;
        data.Flags = Flags;
        data.Values = StringNameValues;

        Data = data;
    }

    /// <inheritdoc cref="ModdedFlagsOption(string, string, T, string, T[], string[], string, string, bool)"/>
    public ModdedFlagsOption(string groupPrefix, string title, T defaultValue, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : this(groupPrefix, title, defaultValue, string.Empty, allowedValues, values, noneId, allId, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedFlagsOption(string, string, T, string, T[], string[], string, string, bool)"/>
    public ModdedFlagsOption(string title, T defaultValue, string idPrefix, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, idPrefix, allowedValues, values, noneId, allId, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedFlagsOption(string, string, T, string, T[], string[], string, string, bool)"/>
    public ModdedFlagsOption(string title, T defaultValue, T[]? allowedValues = null, string[]? values = null, string? noneId = null, string? allId = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, string.Empty, allowedValues, values, noneId, allId, includeInPreset)
    {
    }

    /// <inheritdoc/>
    public override OptionBehaviour CreateOption(ToggleOption toggleOpt, NumberOption numberOpt, StringOption stringOpt, PlayerOption playerOpt, Transform container)
    {
        var multiSelectOption = GameOptionsMenuPatch.CreateOption(stringOpt, container);
        multiSelectOption.name = $"{ParentMod!.OptionsTitleText}.FlagsOption.{Title}";

        multiSelectOption.SetUpFromData(Data, 20);

        multiSelectOption.Option = this;
        multiSelectOption.OnValueChanged = (Action<OptionBehaviour>)ValueChanged;

        OptionBehaviour = multiSelectOption;

        return multiSelectOption;
    }

    /// <inheritdoc/>
    public override float GetFloatData()
    {
        return value;
    }

    /// <inheritdoc/>
    public override NetData GetNetData()
    {
        var buffer = new byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref buffer[0], _value);
        return new(Id, buffer);
    }

    /// <inheritdoc/>
    public override void HandleNetData(byte[] data)
    {
        SetValue(Unsafe.ReadUnaligned<T>(ref data[0]));
    }

    /// <inheritdoc/>
    public override string GetDisplayValue()
    {
        if (value == 0)
        {
            return noneTranslationId.Translate();
        }

        if (value == All)
        {
            return allTranslationId.Translate();
        }

        var activeCount = 0;
        var firstActiveName = string.Empty;
        var foundFirst = false;

        for (var i = 0; i < Flags.Length; i++)
        {
            var flag = Flags[i];

            if (flag == 0 || (value & flag) != flag)
                continue;

            if (!foundFirst)
            {
                firstActiveName = Values[i];
                foundFirst = true;
            }

            activeCount++;
        }

        if (activeCount == 0)
        {
            return noneTranslationId.Translate();
        }

        var firstNameString = firstActiveName.Translate();

        if (activeCount == 1)
        {
            return firstNameString;
        }

        return "MiraApi.Compound".Translate(firstNameString, activeCount - 1);
    }

    /// <inheritdoc/>
    public override T GetValueFromOptionBehaviour(OptionBehaviour optionBehaviour)
    {
        return (T)Enum.ToObject(typeof(T), optionBehaviour.GetInt());
    }

    /// <inheritdoc/>
    protected override void OnValueChanged(T newValue)
    {
        if (OptionBehaviour && OptionBehaviour is FlagsOption opt)
        {
            opt.Value = value;
        }
    }

    /// <inheritdoc/>
    protected override ConfigDescription? CreateDescription()
    {
        return new(string.Empty, new AcceptableFlagValues<T>(AllowedValues));
    }

    /// <inheritdoc/>
    protected override T ModifyValue(T newValue, T oldValue)
    {
        value = Convert.ToInt32(newValue, NumberFormatInfo.InvariantInfo) & All;
        return (T)Enum.ToObject(typeof(T), value);
    }

    /// <summary>
    /// Implicitly converts the option to an <see langword="int"/>.
    /// </summary>
    /// <param name="option">The option.</param>
    /// <returns>Integer value.</returns>
    public static implicit operator int(ModdedFlagsOption<T> option)
    {
        return option.value;
    }
}

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using MiraAPI.Networking;
using MiraAPI.Translation;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// The base class for enum options. Exists mainly to get rid of an abstract implementation.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
/// <param name="title">The title of the option.</param>
/// <param name="defaultValue">The default value as an int.</param>
/// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
public abstract class ModdedBaseEnumOption<T>(string groupPrefix, string title, T defaultValue, bool includeInPreset = true)
    : ModdedBaseStringOption<T>(groupPrefix, title, defaultValue, includeInPreset)
{
    /// <inheritdoc />
    protected override string OptionTypeName => "EnumOption";
}

/// <summary>
/// An option for selecting an <see langword="enum"/> value.
/// </summary>
[Obsolete("Use ModdedEnumOption<T> instead. This will be removed in a future version.")]
public class ModdedEnumOption : ModdedBaseEnumOption<int>
{
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
        idPrefix = idPrefix.Replace("%type%", enumType.Name);

        var rawValues = Enum.GetValues(enumType);
        UnderlyingValues = new int[rawValues.Length];

        for (var i = 0; i < rawValues.Length; i++)
            UnderlyingValues[i] = Convert.ToInt32(rawValues.GetValue(i), NumberFormatInfo.InvariantInfo);

        Values = values ?? Enum.GetNames(enumType);
        StringNameValues = [.. Values.Select(x => MiraLocaleManager.GetOrCreateLocaleString($"{idPrefix}{x}"))];

        InitializeData(GetIndex(defaultValue));
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
    public override NetData GetNetData()
    {
        return new(Id, BitConverter.GetBytes(Value));
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(BitConverter.ToInt32(data));
    }
}

/// <summary>
/// An option for selecting an <see langword="enum"/> value.
/// </summary>
/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
public class ModdedEnumOption<T> : ModdedBaseEnumOption<T>
    where T : struct, Enum
{
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
        idPrefix = idPrefix.Replace("%type%", typeof(T).Name);
        UnderlyingValues = Enum.GetValues<T>();
        Values = values ?? Enum.GetNames<T>();
        StringNameValues = [.. Values.Select(x => MiraLocaleManager.GetOrCreateLocaleString($"{idPrefix}{x}"))];

        InitializeData(GetIndex(defaultValue));
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
    public override NetData GetNetData()
    {
        var buffer = new byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref buffer[0], Value);
        return new(Id, buffer);
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(Unsafe.ReadUnaligned<T>(ref data[0]));
    }
}

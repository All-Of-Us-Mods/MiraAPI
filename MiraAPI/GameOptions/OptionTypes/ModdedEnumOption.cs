using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.Translation;
using MiraAPI.Utilities;

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
        if (!enumType.IsEnum)
            throw new ArgumentException($"The type {enumType.Name} is not an Enum.", nameof(enumType));

        var rawValues = Enum.GetValues(enumType);
        var underlyingValues = new int[rawValues.Length];

        for (var i = 0; i < rawValues.Length; i++)
            underlyingValues[i] = Convert.ToInt32(rawValues.GetValue(i), NumberFormatInfo.InvariantInfo);

        UnderlyingValues = [.. underlyingValues.Distinct()];

        if (values != null && values.Length != UnderlyingValues.Length)
        {
            throw new ArgumentException($"The number of custom values ({values.Length}) must exactly match the number of enum values ({UnderlyingValues.Length}) in {enumType.Name}.", nameof(values));
        }

        if (Array.IndexOf(UnderlyingValues, defaultValue) == -1)
        {
            throw new ArgumentException($"The defaultValue '{defaultValue}' is not present in the allowed enum values.", nameof(defaultValue));
        }

        idPrefix = idPrefix.Replace("%type%", enumType.Name);

        Values = [.. (values ?? rawValues.Cast<object>()).Select(x => idPrefix + x)];

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
        return new(Id, BitConverter.GetBytes(_value));
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(BitConverter.ToInt32(data));
    }

    /// <inheritdoc />
    protected override ConfigDescription? CreateDescription()
    {
        return new(string.Empty, new AcceptableValueList<int>(UnderlyingValues));
    }
}

/// <summary>
/// An option for selecting an <see langword="enum"/> value.
/// </summary>
/// <typeparam name="T">The <see cref="Enum"/> type.</typeparam>
public class ModdedEnumOption<T> : ModdedBaseEnumOption<T>
    where T : struct, Enum
{
    private const string ObsoleteMsg = "This constructor is deprecated. Please use the equivalent constructor that includes the 'T[]? allowedValues' parameter. This will be removed in a future version.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedEnumOption{T}"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as an <see langword="int"/>.</param>
    /// <param name="idPrefix">The common translation ID prefix of all enum values.</param>
    /// <param name="allowedValues">An option array of enum values to filter down what values can actually be used.</param>
    /// <param name="values">An optional array of string values to use in place of the <see langword="enum"/> name.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, string idPrefix, T[]? allowedValues = null, string[]? values = null, bool includeInPreset = true)
        : base(groupPrefix, title, defaultValue, includeInPreset)
    {
        UnderlyingValues = [.. (allowedValues ?? Enum.GetValues<T>()).Distinct()];

        if (values != null && values.Length != UnderlyingValues.Length)
        {
            throw new ArgumentException($"The number of custom values ({values.Length}) must exactly match the number of enum values ({UnderlyingValues.Length}) in {typeof(T).Name}.", nameof(values));
        }

        if (Array.IndexOf(UnderlyingValues, defaultValue) == -1)
        {
            throw new ArgumentException($"The defaultValue '{defaultValue}' is not present in the allowed enum values.", nameof(defaultValue));
        }

        idPrefix = idPrefix.Replace("%type%", typeof(T).Name);

        Values = [.. (values ?? UnderlyingValues.Cast<object>()).Select(x => idPrefix + x)];

        InitializeData(GetIndex(defaultValue));
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, T[]? allowedValues = null, string[]? values = null, bool includeInPreset = true)
        : this(groupPrefix, title, defaultValue, string.Empty, allowedValues, values, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    public ModdedEnumOption(string title, T defaultValue, T[]? allowedValues = null, string[]? values = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, string.Empty, allowedValues, values, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    public ModdedEnumOption(string title, T defaultValue, string idPrefix, T[]? allowedValues = null, string[]? values = null, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, idPrefix, allowedValues, values, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, string idPrefix, string[] values, bool includeInPreset)
        : this(groupPrefix, title, defaultValue, idPrefix, null, values, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, string idPrefix, string[] values)
        : this(groupPrefix, title, defaultValue, idPrefix, null, values, true)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, string[] values, bool includeInPreset)
        : this(groupPrefix, title, defaultValue, string.Empty, null, values, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string groupPrefix, string title, T defaultValue, string[] values)
        : this(groupPrefix, title, defaultValue, string.Empty, null, values, true)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string title, T defaultValue, string[] values, bool includeInPreset)
        : this(string.Empty, title, defaultValue, string.Empty, null, values, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string title, T defaultValue, string[] values)
        : this(string.Empty, title, defaultValue, string.Empty, null, values, true)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string title, T defaultValue, string idPrefix, string[] values, bool includeInPreset)
        : this(string.Empty, title, defaultValue, idPrefix, null, values, includeInPreset)
    {
    }

    /// <inheritdoc cref="ModdedEnumOption{T}(string, string, T, string, T[], string[], bool)"/>
    [Obsolete(ObsoleteMsg)]
    public ModdedEnumOption(string title, T defaultValue, string idPrefix, string[] values)
        : this(string.Empty, title, defaultValue, idPrefix, null, values, true)
    {
    }

    /// <inheritdoc />
    public override NetData GetNetData()
    {
        var buffer = new byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref buffer[0], _value);
        return new(Id, buffer);
    }

    /// <inheritdoc />
    public override void HandleNetData(byte[] data)
    {
        SetValue(Unsafe.ReadUnaligned<T>(ref data[0]));
    }

    /// <inheritdoc />
    protected override ConfigDescription? CreateDescription()
    {
        return new(string.Empty, new AcceptableEnumValues<T>(UnderlyingValues));
    }
}

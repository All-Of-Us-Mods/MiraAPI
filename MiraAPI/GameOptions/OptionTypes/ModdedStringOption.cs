using System;
using System.Linq;
using BepInEx.Configuration;
using MiraAPI.Networking;
using MiraAPI.Translation;

namespace MiraAPI.GameOptions.OptionTypes;

/// <summary>
/// An option for selecting a string value from a predefined list.
/// </summary>
public class ModdedStringOption : ModdedBaseStringOption<string>
{
    /// <inheritdoc/>
    protected override string OptionTypeName => "StringOption";

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedStringOption"/> class.
    /// </summary>
    /// <param name="groupPrefix">The translation ID prefix of the group containing this option.</param>
    /// <param name="title">The title of the option.</param>
    /// <param name="defaultValue">The default value as a string.</param>
    /// <param name="values">An option list of string values to use.</param>
    /// <param name="includeInPreset">Whether to include this option in the preset or not.</param>
    public ModdedStringOption(string groupPrefix, string title, string defaultValue, string[] values, bool includeInPreset = true)
        : base(groupPrefix, title, defaultValue, includeInPreset)
    {
        Values = values;
        UnderlyingValues = values;

        InitializeData(GetIndex(defaultValue));
    }

    /// <inheritdoc cref="ModdedStringOption(string, string, string, string[], bool)"/>
    public ModdedStringOption(string title, string defaultValue, string[] values, bool includeInPreset = true)
        : this(string.Empty, title, defaultValue, values, includeInPreset)
    {
    }

    /// <summary>
    /// Adds a value to the option dynamically.
    /// </summary>
    /// <param name="value">The option's string value.</param>
    /// <param name="index">The additional value's index on the list.</param>
    [Obsolete("Use the Setter feature of modded options to simulate conditional values. This wil be removed in a future version.")]
    public void RegisterExtraValue(string value, int index = -1)
    {
        Warning("Use the Setter feature of modded options to simulate conditional values.");

        if (!Values.Contains(value))
        {
            if (index <= -1)
                index = Values.Length;

            var newVals = Values.ToList();
            newVals.Insert(index, value);
            Values = [.. newVals];
            UnderlyingValues = Values;

            var newStrVals = StringNameValues.ToList();
            newStrVals.Insert(index, MiraLocaleManager.GetOrCreateLocaleString(value));
            StringNameValues = [.. newStrVals];
        }

        if (Data == null)
        {
            return;
        }

        var data = (StringGameSetting)Data;
        data.Values = StringNameValues;
        data.Index = GetIndex(Value);

        if (OptionBehaviour == null)
        {
            return;
        }

        var strOpt = OptionBehaviour.TryCast<StringOption>();

        if (strOpt == null)
        {
            return;
        }

        strOpt.Values = StringNameValues;
        strOpt.Value = GetIndex(Value);
    }

    /// <inheritdoc/>
    public override NetData GetNetData()
    {
        return new NetData(Id, BitConverter.GetBytes(GetIndex(_value)));
    }

    /// <inheritdoc/>
    public override void HandleNetData(byte[] data)
    {
        SetValue(UnderlyingValues[BitConverter.ToInt32(data)]);
    }

    /// <inheritdoc/>
    protected override ConfigDescription? CreateDescription()
    {
        return new(string.Empty, new AcceptableValueList<string>(UnderlyingValues));
    }
}

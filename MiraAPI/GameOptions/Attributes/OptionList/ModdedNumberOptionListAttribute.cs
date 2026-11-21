using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of number options.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="min">The minimum value.</param>
/// <param name="max">The maximum value.</param>
/// <param name="increment">The increment.</param>
/// <param name="zeroBehavior">Determines what is shown for zero.</param>
/// <param name="negativeBehavior">Determines what is shown for negative 1.</param>
/// <param name="suffixType">The suffix type.</param>
/// <param name="formatString">Optional format string for the option screen.</param>
/// <param name="halfIncrements">Whether increments can be split in half (using Shift).</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedNumberOptionListAttribute(
    string? title,
    float min,
    float max,
    float increment,
    string zeroBehavior,
    string negativeBehavior,
    MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
    string? formatString = null,
    bool halfIncrements = false)
    : ModdedOptionListAttribute(title), ITypedOptionList<int>, ITypedOptionList<float>
{
    /// <inheritdoc cref="ModdedNumberOptionListAttribute(string, float, float, float, string, string, MiraNumberSuffixes, string, bool)"/>
    public ModdedNumberOptionListAttribute(
        float min,
        float max,
        float increment,
        string zeroBehavior,
        string negativeBehavior,
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        bool halfIncrements = false)
        : this(null, min, max, increment, zeroBehavior, negativeBehavior, suffixType, formatString, halfIncrements)
    {
    }

    /// <inheritdoc cref="ModdedNumberOptionListAttribute(string, float, float, float, string, string, MiraNumberSuffixes, string, bool)"/>
    /// <param name="zeroInfinity">Whether zero is infinity or not.</param>
    public ModdedNumberOptionListAttribute(
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do) (Justification: Inherited.)
        string? title,
        float min,
        float max,
        float increment = 1,
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        bool zeroInfinity = false)
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        : this(title, min, max, increment, zeroInfinity ? "∞" : "#", "#", suffixType, formatString, increment == 1f)
    {
    }

    /// <inheritdoc cref="ModdedNumberOptionListAttribute(string, float, float, float, MiraNumberSuffixes, string, bool)"/>
    public ModdedNumberOptionListAttribute(
        float min,
        float max,
        float increment = 1,
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        bool zeroInfinity = false)
        : this(null, min, max, increment, suffixType, formatString, zeroInfinity)
    {
    }

    internal override IBaseModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        return new ModdedOptionList<ModdedNumberOption>(list.Count, idx =>
        {
            var final = list[idx] switch
            {
                null => min + increment,
                int integer => integer,
                _ => (float)list[idx]!,
            };

            return new(
                group.GroupPrefix,
                GetFormattedTitle(idx, property),
                final,
                min,
                max,
                increment,
                zeroBehavior,
                negativeBehavior,
                suffixType,
                formatString,
                halfIncrements);
        });
    }

    /// <inheritdoc/>
    public override void SetValue(int idx, object value)
    {
        if (HolderOptionList?.ElementAt(idx) is not ModdedNumberOption opt)
            return;

        var final = value switch
        {
            int integer => integer,
            _ => (float)value,
        };
        opt.SetValue(final);
    }

    /// <inheritdoc/>
    public void SetValueUnboxed(int idx, float value)
    {
        (HolderOptionList?.ElementAt(idx) as ModdedNumberOption)?.SetValue(value);
    }

    /// <inheritdoc/>
    void ITypedOptionList<int>.SetValueUnboxed(int idx, int value)
    {
        (HolderOptionList?.ElementAt(idx) as ModdedNumberOption)?.SetValue(value);
    }

    /// <inheritdoc/>
    public override object GetValue(int idx)
    {
        if (HolderOptionList?.ElementAt(idx) is not ModdedNumberOption opt)
            throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedNumberOption");

        var value = opt.Value;

        if (ValueType == typeof(int))
            return (int)MathF.Round(value, MidpointRounding.AwayFromZero);

        return value;
    }

    /// <inheritdoc/>
    public float GetValueUnboxed(int idx)
    {
        return HolderOptionList?.ElementAt(idx) is ModdedNumberOption opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedNumberOption");
    }

    /// <inheritdoc/>
    int ITypedOptionList<int>.GetValueUnboxed(int idx)
    {
        return (int)MathF.Round(GetValueUnboxed(idx), MidpointRounding.AwayFromZero);
    }
}

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
    string title,
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
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedNumberOptionListAttribute"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="zeroInfinity">Whether zero is infinity or not.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    public ModdedNumberOptionListAttribute(
        string title,
        float min,
        float max,
        float increment = 1,
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        bool zeroInfinity = false)
        : this(title, min, max, increment, zeroInfinity ? "∞" : "#", "#", suffixType, formatString, increment == 1f)
    {
    }

    internal override IModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        return new ModdedOptionList<ModdedNumberOption>(value.Count, idx =>
        {
            var final = value[idx] switch
            {
                null => min + increment,
                int integer => integer,
                _ => (float)value[idx]!,
            };

            return new(
                GetFormattedTitle(idx),
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

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        if (HolderOptionList?[idx] is not ModdedNumberOption opt)
            return;

        var final = value switch
        {
            int integer => integer,
            _ => (float)value,
        };
        opt.SetValue(final);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(int idx, float value)
    {
        (HolderOptionList?[idx] as ModdedNumberOption)?.SetValue(value);
    }

    /// <inheritdoc />
    void ITypedOptionList<int>.SetValueUnboxed(int idx, int value)
    {
        (HolderOptionList?[idx] as ModdedNumberOption)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        if (HolderOptionList?[idx] is not ModdedNumberOption opt)
            throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedNumberOption");

        var value = opt.Value;

        if (ValueType == typeof(int))
            return (int)MathF.Round(value);

        return value;
    }

    /// <inheritdoc />
    public float GetValueUnboxed(int idx)
    {
        return HolderOptionList?[idx] is ModdedNumberOption opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedNumberOption");
    }

    /// <inheritdoc />
    int ITypedOptionList<int>.GetValueUnboxed(int idx)
    {
        return (int)Math.Round(GetValueUnboxed(idx));
    }
}

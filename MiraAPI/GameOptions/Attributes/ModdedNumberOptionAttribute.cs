using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a <see cref="ModdedNumberOption"/>.
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
public class ModdedNumberOptionAttribute(
    string? title,
    float min,
    float max,
    float increment,
    string zeroBehavior,
    string negativeBehavior,
    MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
    string? formatString = null,
    bool halfIncrements = false)
    : ModdedOptionAttribute(title), ITypedOption<int>, ITypedOption<float>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedNumberOptionAttribute"/> class.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="zeroBehavior">Determines what is shown for zero.</param>
    /// <param name="negativeBehavior">Determines what is shown for negative 1.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    /// <param name="halfIncrements">Whether increments can be split in half (using Shift).</param>
    public ModdedNumberOptionAttribute(
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedNumberOptionAttribute"/> class.
    /// </summary>
    /// <param name="title">The title of the option.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="zeroInfinity">Whether zero is infinity or not.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    public ModdedNumberOptionAttribute(
        string? title,
        float min,
        float max,
        float increment = 1,
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        bool zeroInfinity = false)
        : this(title, min, max, increment, zeroInfinity ? "∞" : "#", "#", suffixType, formatString, increment == 1f)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedNumberOptionAttribute"/> class.
    /// </summary>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="suffixType">The suffix type.</param>
    /// <param name="zeroInfinity">Whether zero is infinity or not.</param>
    /// <param name="formatString">Optional format string for the option screen.</param>
    public ModdedNumberOptionAttribute(
        float min,
        float max,
        float increment = 1,
        MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
        string? formatString = null,
        bool zeroInfinity = false)
        : this(null, min, max, increment, suffixType, formatString, zeroInfinity)
    {
    }

    internal override IModdedOption CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group)
    {
        var final = value switch
        {
            null => min + increment,
            int integer => integer,
            _ => (float)value,
        };

        var opt = new ModdedNumberOption(
            group.GroupPrefix,
            Title ?? property.Name,
            final,
            min,
            max,
            increment,
            zeroBehavior,
            negativeBehavior,
            suffixType,
            formatString,
            halfIncrements);
        Title = opt.Title;
        return opt;
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        if (HolderOption is not ModdedNumberOption opt)
            return;

        var final = value switch
        {
            int integer => integer,
            _ => (float)value,
        };
        opt.SetValue(final);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(float value)
    {
        (HolderOption as ModdedNumberOption)?.SetValue(value);
    }

    /// <inheritdoc />
    void ITypedOption<int>.SetValueUnboxed(int value)
    {
        (HolderOption as ModdedNumberOption)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        if (HolderOption is not ModdedNumberOption opt)
            throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedNumberOption");

        var value = opt.Value;

        if (ValueType == typeof(int))
            return (int)MathF.Round(value);

        return value;
    }

    /// <inheritdoc />
    public float GetValueUnboxed()
    {
        return HolderOption is ModdedNumberOption opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Title}\" is not a ModdedNumberOption");
    }

    /// <inheritdoc />
    int ITypedOption<int>.GetValueUnboxed()
    {
        return (int)Math.Round(GetValueUnboxed());
    }
}

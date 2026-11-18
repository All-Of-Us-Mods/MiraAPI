using System;
using BepInEx.Configuration;

namespace MiraAPI.Utilities;

/// <summary>
/// Specify the range of acceptable values for a setting.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed class SafeAcceptableValueRange<T> : AcceptableValueBase
    where T : IComparable<T>
{
    /// <summary>
    /// Gets the lowest acceptable value.
    /// </summary>
    public T Min { get; }

    /// <summary>
    /// Gets the highest acceptable value.
    /// </summary>
    public T Max { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeAcceptableValueRange{T}"/> class.
    /// </summary>
    /// <param name="min">Lowest acceptable value.</param>
    /// <param name="max">Highest acceptable value.</param>
    public SafeAcceptableValueRange(T min, T max)
        : base(typeof(T))
    {
        if (min is null)
        {
            throw new ArgumentNullException(nameof(min));
        }

        if (max is null)
        {
            throw new ArgumentNullException(nameof(max));
        }

        if (min.CompareTo(max) > 0)
        {
            Min = max;
            Max = min;
        }
        else
        {
            Min = min;
            Max = max;
        }
    }

    /// <inheritdoc/>
    public override object Clamp(object value)
    {
        if (value is not T val)
            return Min;

        if (val.CompareTo(Min) <= 0)
            return Min;

        if (val.CompareTo(Max) >= 0)
            return Max;

        return val;
    }

    /// <inheritdoc/>
    public override bool IsValid(object value)
    {
        if (value is not T val)
            return false;

        return val.CompareTo(Min) >= 0 && val.CompareTo(Max) <= 0;
    }

    /// <inheritdoc/>
    public override string ToDescriptionString()
    {
        return $"# Acceptable value range: From {Min} to {Max}";
    }
}

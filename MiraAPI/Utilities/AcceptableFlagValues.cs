using System;
using System.Globalization;
using BepInEx.Configuration;

namespace MiraAPI.Utilities;

/// <summary>
/// Specify the list of acceptable individual flag values for a setting.<br/>
/// Any bitwise combination of these provided flags is considered valid.
/// </summary>
/// <typeparam name="T">The type of the flag enum.</typeparam>
public sealed class AcceptableFlagValues<T> : AcceptableValueBase
    where T : struct, Enum
{
    /// <summary>
    /// Gets a list of single-bit values that a setting can include.
    /// </summary>
    public T[] AcceptableValues { get; }

    private readonly int allMask;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptableFlagValues{T}"/> class.
    /// </summary>
    /// <param name="acceptableValues">The acceptable individual flags.</param>
    /// <exception cref="ArgumentNullException">Thrown when the given values are null.</exception>
    /// <exception cref="ArgumentException">Thrown when no values are given.</exception>
    public AcceptableFlagValues(params T[] acceptableValues)
        : base(typeof(T))
    {
        ArgumentNullException.ThrowIfNull(acceptableValues);

        if (acceptableValues.Length == 0)
            throw new ArgumentException("At least one acceptable value is needed", nameof(acceptableValues));

        AcceptableValues = acceptableValues;

        allMask = 0;

        foreach (var val in acceptableValues)
            allMask |= Convert.ToInt32(val, NumberFormatInfo.InvariantInfo);
    }

    /// <inheritdoc/>
    public override object Clamp(object value)
    {
        if (IsValid(value))
            return value;

        if (value is not T)
            return Enum.ToObject(typeof(T), 0);

        var intVal = Convert.ToInt32(value, NumberFormatInfo.InvariantInfo);
        var clampedVal = intVal & allMask;
        return Enum.ToObject(typeof(T), clampedVal);
    }

    /// <inheritdoc/>
    public override bool IsValid(object value)
    {
        if (value is not T)
            return false;

        var intVal = Convert.ToInt32(value, NumberFormatInfo.InvariantInfo);
        return (intVal & ~allMask) == 0;
    }

    /// <inheritdoc/>
    public override string ToDescriptionString()
    {
        return "# Acceptable Flags: " + string.Join(", ", AcceptableValues);
    }
}

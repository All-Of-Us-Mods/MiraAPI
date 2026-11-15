using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace MiraAPI.Utilities;

/// <summary>
/// Specify the list of acceptable enum values for a setting.
/// </summary>
/// <typeparam name="T">The type of the enum.</typeparam>
public sealed class AcceptableEnumValues<T> : AcceptableValueBase
    where T : struct, Enum
{
    /// <summary>
    /// Gets a list of values that a setting can take.
    /// </summary>
    public T[] AcceptableValues { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptableEnumValues{T}"/> class.
    /// </summary>
    /// <param name="acceptableValues">The acceptable values.</param>
    /// <exception cref="ArgumentNullException">Thrown when the given values are null.</exception>
    /// <exception cref="ArgumentException">Thrown when no values are given.</exception>
    public AcceptableEnumValues(params T[] acceptableValues)
        : base(typeof(T))
    {
        ArgumentNullException.ThrowIfNull(acceptableValues);

        if (acceptableValues.Length == 0)
        {
            throw new ArgumentException("At least one acceptable value is needed", nameof(acceptableValues));
        }

        AcceptableValues = acceptableValues;
    }

    /// <inheritdoc/>
    public override object Clamp(object value)
    {
        if (IsValid(value))
        {
            return value;
        }

        return AcceptableValues[0];
    }

    /// <inheritdoc/>
    public override bool IsValid(object value)
    {
        if (value is not T v)
        {
            return false;
        }

        return AcceptableValues.Any(x => EqualityComparer<T>.Default.Equals(x, v));
    }

    /// <inheritdoc/>
    public override string ToDescriptionString()
    {
        return "# Acceptable values: " + string.Join(", ", AcceptableValues.Select(x => x.ToString()));
    }
}

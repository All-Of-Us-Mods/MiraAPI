using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MiraAPI.Utilities;

/// <summary>
/// Defines a method that a type implements to compare two objects using extra contextual information.
/// </summary>
/// <typeparam name="TValue">The type of objects to compare.</typeparam>
/// <typeparam name="TParam">The type of the context.</typeparam>
public interface IComparer<in TValue, in TParam>
{
    /// <summary>
    /// When overridden in a derived class, performs a comparison of two objects of the
    /// same type using the provided context and returns a value indicating whether one
    /// object is less than, equal or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <param name="context">The extra information to aid in the comparison.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of x and y, as shown in the
    /// following table.<br/><br/>
    /// Value – Meaning<br/>
    /// Less than zero – x is less than y.<br/>
    /// Zero – x equals y.<br/>
    /// Greater than zero – x is greater than y.<br/>
    /// </returns>
    int Compare(TValue? x, TValue? y, TParam? context);
}

/// <summary>
/// Provides a base class for implementations of the <see cref="IComparer{TValue, TParam}"/>
/// generic interface.
/// </summary>
/// <typeparam name="TValue">The type of objects to compare.</typeparam>
/// <typeparam name="TParam">The type of the context.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Mimics Comparer<T> design.")]
public abstract class Comparer<TValue, TParam> : IComparer<TValue, TParam>
{
    /// <summary>
    /// Gets a default contextual comparer that ignores the parameter and falls back to standard comparison.
    /// </summary>
    public static Comparer<TValue, TParam> Default { get; } = new DefaultComparer();

    /// <summary>
    /// Creates a contextual comparer by using the specified comparison delegate.
    /// </summary>
    /// <param name="comparison">The comparison to use.</param>
    /// <returns>The new comparer.</returns>
    public static Comparer<TValue, TParam> Create(Func<TValue?, TValue?, TParam?, int>? comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        return new FuncComparer(comparison);
    }

    /// <inheritdoc/>
    public abstract int Compare(TValue? x, TValue? y, TParam? context);

    private sealed class DefaultComparer : Comparer<TValue, TParam>
    {
        public override int Compare(TValue? x, TValue? y, TParam? context)
        {
            return Comparer<TValue>.Default.Compare(x, y);
        }
    }

    private sealed class FuncComparer(Func<TValue?, TValue?, TParam?, int> comparison) : Comparer<TValue, TParam>
    {
        public override int Compare(TValue? x, TValue? y, TParam? context)
        {
            return comparison(x, y, context);
        }
    }
}

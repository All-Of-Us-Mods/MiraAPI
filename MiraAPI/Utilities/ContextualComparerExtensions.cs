using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Utilities;

/// <summary>
/// Provides extension methods to integrate <see cref="IComparer{TValue, TParam}"/> with standard .NET collections and LINQ.
/// </summary>
public static class ContextualComparerExtensions
{
    private readonly struct BoundComparer<T, TParam>(IComparer<T, TParam> comparer, TParam? context) : IComparer<T>
    {
        public int Compare(T? x, T? y)
        {
            return comparer.Compare(x, y, context);
        }
    }

    /// <summary>
    /// Binds a context parameter to a contextual comparer, returning a standard <see cref="IComparer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="comparer">The contextual comparer.</param>
    /// <param name="context">The context to bind to the comparer.</param>
    /// <returns>A standard comparer that implicitly uses the provided context.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
    public static IComparer<T> Bind<T, TParam>(this IComparer<T, TParam> comparer, TParam? context)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        return new BoundComparer<T, TParam>(comparer, context);
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="List{T}"/> using the specified contextual comparer and context.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="list">The list to sort.</param>
    /// <param name="comparer">The contextual comparer to use when comparing elements.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="comparer"/> is null.</exception>
    public static void Sort<T, TParam>(this List<T> list, IComparer<T, TParam> comparer, TParam? context)
    {
        ArgumentNullException.ThrowIfNull(list);
        list.Sort(comparer.Bind(context));
    }

    /// <summary>
    /// Sorts the elements in an entire one-dimensional array using the specified contextual comparer and context.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="array">The one-dimensional array to sort.</param>
    /// <param name="comparer">The contextual comparer to use when comparing elements.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> or <paramref name="comparer"/> is null.</exception>
    public static void Sort<T, TParam>(this T[] array, IComparer<T, TParam> comparer, TParam? context)
    {
        ArgumentNullException.ThrowIfNull(array);
        Array.Sort(array, comparer.Bind(context));
    }

    /// <summary>
    /// Searches the entire sorted <see cref="List{T}"/> for an element using the specified contextual comparer and context,
    /// and returns the zero-based index of the element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="list">The sorted list to search.</param>
    /// <param name="item">The object to locate. The value can be null for reference types.</param>
    /// <param name="comparer">The contextual comparer to use when comparing elements.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <returns>
    /// The zero-based index of item in the sorted list, if item is found; otherwise, a negative number that is the
    /// bitwise complement of the index of the next element that is larger than item or, if there is no larger element,
    /// the bitwise complement of Count.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> or <paramref name="comparer"/> is null.</exception>
    public static int BinarySearch<T, TParam>(this List<T> list, T item, IComparer<T, TParam> comparer, TParam? context)
    {
        ArgumentNullException.ThrowIfNull(list);
        return list.BinarySearch(item, comparer.Bind(context));
    }

    /// <summary>
    /// Searches an entire one-dimensional sorted array for a specific element using the specified contextual comparer and context.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="array">The sorted one-dimensional array to search.</param>
    /// <param name="item">The object to locate. The value can be null for reference types.</param>
    /// <param name="comparer">The contextual comparer to use when comparing elements.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <returns>
    /// The index of the specified value in the specified array, if value is found; otherwise, a negative number.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> or <paramref name="comparer"/> is null.</exception>
    public static int BinarySearch<T, TParam>(this T[] array, T item, IComparer<T, TParam> comparer, TParam? context)
    {
        ArgumentNullException.ThrowIfNull(array);
        return Array.BinarySearch(array, item, comparer.Bind(context));
    }

    /// <summary>
    /// Sorts the elements of a sequence in ascending order according to a key and a contextual comparer.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="source">A sequence of values to order.</param>
    /// <param name="keySelector">A function to extract a key from an element.</param>
    /// <param name="comparer">The contextual comparer to compare keys.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted according to a key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="comparer"/> is null.</exception>
    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey, TParam>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey, TParam> comparer,
        TParam? context)
    {
        return source.OrderBy(keySelector, comparer.Bind(context));
    }

    /// <summary>
    /// Sorts the elements of a sequence in descending order according to a key and a contextual comparer.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="source">A sequence of values to order.</param>
    /// <param name="keySelector">A function to extract a key from an element.</param>
    /// <param name="comparer">The contextual comparer to compare keys.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted in descending order according to a key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="comparer"/> is null.</exception>
    public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey, TParam>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey, TParam> comparer,
        TParam? context)
    {
        return source.OrderByDescending(keySelector, comparer.Bind(context));
    }

    /// <summary>
    /// Performs a subsequent ordering of the elements in a sequence in ascending order according to a key and a contextual comparer.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="source">An <see cref="IOrderedEnumerable{TElement}"/> that contains elements to sort.</param>
    /// <param name="keySelector">A function to extract a key from an element.</param>
    /// <param name="comparer">The contextual comparer to compare keys.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted according to a key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="comparer"/> is null.</exception>
    public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey, TParam>(
        this IOrderedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey, TParam> comparer,
        TParam? context)
    {
        return source.ThenBy(keySelector, comparer.Bind(context));
    }

    /// <summary>
    /// Performs a subsequent ordering of the elements in a sequence in descending order, according to a key and a contextual comparer.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
    /// <typeparam name="TParam">The type of the context.</typeparam>
    /// <param name="source">An <see cref="IOrderedEnumerable{TElement}"/> that contains elements to sort.</param>
    /// <param name="keySelector">A function to extract a key from an element.</param>
    /// <param name="comparer">The contextual comparer to compare keys.</param>
    /// <param name="context">The context to aid in the comparison.</param>
    /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted in descending order according to a key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="comparer"/> is null.</exception>
    public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey, TParam>(
        this IOrderedEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey, TParam> comparer,
        TParam? context)
    {
        return source.ThenByDescending(keySelector, comparer.Bind(context));
    }
}

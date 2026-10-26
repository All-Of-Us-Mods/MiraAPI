using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.GameOptions;

/// <summary>
/// A fixed-size array created specifically for modded options that automatically synchronizes with option attributes.
/// </summary>
/// <typeparam name="T">The type of the values that the array holds.</typeparam>
[CollectionBuilder(typeof(ModdedArrayBuilder), nameof(ModdedArrayBuilder.Create))]
public sealed class ModdedArray<T> : IList<T>, IReadOnlyList<T>, IList
{
    private readonly T[] items;
    private int version;

    /// <inheritdoc/>
    public int Count => items.Length;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    bool IList.IsFixedSize => true;

    /// <inheritdoc/>
    bool ICollection.IsSynchronized => false;

    /// <inheritdoc/>
    object ICollection.SyncRoot => items.SyncRoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedArray{T}"/> class with the provided capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the list.</param>
    public ModdedArray(int capacity)
    {
        items = new T[capacity];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedArray{T}"/> class with initial values.
    /// </summary>
    /// <param name="collection">The initial collection of values to initialise with.</param>
    public ModdedArray(IEnumerable<T> collection)
    {
        items = [.. collection];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedArray{T}"/> class with initial values.
    /// </summary>
    /// <param name="span">The initial span of values to initialise with.</param>
    public ModdedArray(Span<T> span)
    {
        items = [.. span];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedArray{T}"/> class with initial values.
    /// </summary>
    /// <param name="span">The initial span of values to initialise with.</param>
    public ModdedArray(ReadOnlySpan<T> span)
    {
        items = [.. span];
    }

    /// <summary>
    /// Gets or sets the element at the specified index, syncing with the attribute if registered.
    /// </summary>
    /// <param name="index">The index of the value.</param>
    public T this[int index]
    {
        get
        {
            if (!ModdedOptionsManager.ListInstanceCache.TryGetValue(this, out var attribute))
                return items[index];

            return attribute is ITypedOptionList<T> typedAttr
                ? typedAttr.GetValueUnboxed(index)
                : (T)attribute.GetValue(index);
        }
        set
        {
            if (ModdedOptionsManager.ListInstanceCache.TryGetValue(this, out var attribute))
            {
                if (attribute is ITypedOptionList<T> typedAttr)
                    typedAttr.SetValueUnboxed(index, value);
                else
                    attribute.SetValue(index, value!);
            }
            else
            {
                items[index] = value;
            }

            version++;
        }
    }

    /// <inheritdoc/>
    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = (T)value!;
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
        return Array.IndexOf(items, item) >= 0;
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        items.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public int IndexOf(T item)
    {
        return Array.IndexOf(items, item);
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public Enumerator GetEnumerator()
    {
        return new(this);
    }

    /// <inheritdoc/>
    bool IList.Contains(object? value)
    {
        return ((IList)items).Contains(value);
    }

    /// <inheritdoc/>
    int IList.IndexOf(object? value)
    {
        return ((IList)items).IndexOf(value);
    }

    /// <inheritdoc/>
    void ICollection.CopyTo(Array array, int index)
    {
        items.CopyTo(array, index);
    }

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    int IList.Add(object? value)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    void IList.Clear()
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    void IList.Insert(int index, object? value)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    void IList.Remove(object? value)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    void IList.RemoveAt(int index)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    public void Insert(int index, T item)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    public void Add(T item)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    public void Clear()
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        throw new NotSupportedException("ModdedArray is of a fixed size.");
    }

    /// <summary>
    /// Enumerates the elements of a <see cref="ModdedArray{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly ModdedArray<T> array;
        private readonly int version;

        private int index;
        private T? current;

        internal Enumerator(ModdedArray<T> moddedArray)
        {
            array = moddedArray;
            version = moddedArray.version;
            index = 0;
            current = default;
        }

        /// <inheritdoc/>
        public readonly T Current => current!;

        /// <inheritdoc/>
        readonly object? IEnumerator.Current
        {
            get
            {
                if (index == 0 || index == array.Count + 1)
                    throw new InvalidOperationException("Enumeration has not started or has already finished.");

                return Current;
            }
        }

        /// <inheritdoc/>
        public readonly void Dispose()
        {
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (version != array.version)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

            if ((uint)index < (uint)array.Count)
            {
                current = array[index];
                index++;
                return true;
            }

            index = array.Count + 1;
            current = default;
            return false;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            if (version != array.version)
                throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

            index = 0;
            current = default;
        }
    }
}

/// <summary>
/// Provides a builder for <see cref="ModdedArray{T}"/> to support C# 12 collection expressions.
/// </summary>
public static class ModdedArrayBuilder
{
    /// <summary>
    /// Creates a new <see cref="ModdedArray{T}"/> from a span of values.
    /// </summary>
    /// <typeparam name="T">The the type of the values.</typeparam>
    /// <param name="values">The values to initialise with.</param>
    /// <returns>The <see cref="ModdedArray{T}"/> instance initialised with the provided values.</returns>
    public static ModdedArray<T> Create<T>(ReadOnlySpan<T> values)
    {
        return new(values);
    }
}

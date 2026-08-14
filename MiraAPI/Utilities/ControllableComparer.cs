using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MiraAPI.Utilities;

internal sealed class ControllableComparer<T>(T[] forcedToBottom, T[] forcedToTop, IComparer<T> fallbackComparer) : IComparer<T> where T : IComparable
{
    private readonly T[] _forcedToBottom = forcedToBottom;
    private readonly T[] _forcedToTop = forcedToTop;
    private readonly IComparer<T> _fallbackComparer = fallbackComparer;

    /// <inheritdoc/>
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Warning cascades into forcing the entire tree to be ternary operators.")]
    public int Compare(T? x, T? y)
    {
        if ((_forcedToBottom.Contains(x) && _forcedToBottom.Contains(y)) || (_forcedToTop.Contains(x) && _forcedToTop.Contains(y)))
            return _fallbackComparer.Compare(x, y);

        if (_forcedToBottom.Contains(x))
            return 1;

        if (_forcedToBottom.Contains(y))
            return -1;

        if (_forcedToTop.Contains(x))
            return -1;

        if (_forcedToTop.Contains(y))
            return 1;

        return _fallbackComparer.Compare(x, y);
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MiraAPI.Utilities;

internal sealed class ControllableComparer<T>(T[] forcedToBottom, T[] forcedToTop, IComparer<T> fallbackComparer) : IComparer<T> where T : IComparable
{
    /// <inheritdoc/>
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Warning cascades into forcing the entire tree to be ternary operators.")]
    public int Compare(T? x, T? y)
    {
        if ((forcedToBottom.Contains(x) && forcedToBottom.Contains(y)) || (forcedToTop.Contains(x) && forcedToTop.Contains(y)))
            return fallbackComparer.Compare(x, y);

        if (forcedToBottom.Contains(x))
            return 1;

        if (forcedToBottom.Contains(y))
            return -1;

        if (forcedToTop.Contains(x))
            return -1;

        if (forcedToTop.Contains(y))
            return 1;

        return fallbackComparer.Compare(x, y);
    }
}

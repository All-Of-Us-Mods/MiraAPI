using System.Collections.Generic;

namespace MiraAPI.Utilities;

/// <summary>
/// A comparer used to sort role/modifier IDs based on which ones are currently active.
/// </summary>
/// <param name="activeIds">The collection of currently active IDs.</param>
public class BasicGuideComparer(IEnumerable<uint> activeIds) : IComparer<uint>
{
    private readonly HashSet<uint> _activeIds = activeIds as HashSet<uint> ?? [.. activeIds];

    /// <inheritdoc/>
    public int Compare(uint x, uint y)
    {
        var xActive = _activeIds.Contains(x);
        var yActive = _activeIds.Contains(y);
        return yActive.CompareTo(xActive);
    }
}

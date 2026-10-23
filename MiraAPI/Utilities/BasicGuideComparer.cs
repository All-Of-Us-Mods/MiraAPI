using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Utilities;

/// <summary>
/// A comparer used to sort role/modifier IDs based on which ones are currently active.
/// </summary>
/// <param name="activeIds">The collection of currently active IDs.</param>
public class BasicGuideComparer(IEnumerable<uint> activeIds) : IComparer<uint>
{
    /// <inheritdoc/>
    public int Compare(uint x, uint y)
    {
        var xActive = activeIds.Contains(x);
        var yActive = activeIds.Contains(y);

        if (xActive && !yActive)
        {
            return -1;
        }

        if (!xActive && yActive)
        {
            return 1;
        }

        return 0;
    }
}

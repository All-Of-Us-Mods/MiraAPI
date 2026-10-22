using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Utilities;

public class BasicGuideComparer(IEnumerable<uint> activeIds) : IComparer<uint>
{
    public int Compare(uint x, uint y)
    {
        if (x == null && y == null)
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

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
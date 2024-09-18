using System;
using System.Collections.Generic;

namespace MiraAPI.Cosmetics;
internal sealed class AbstractCosmeticsGroupComparer : Comparer<AbstractCosmeticsGroup>
{
    public override int Compare(AbstractCosmeticsGroup? x, AbstractCosmeticsGroup? y)
    {
        switch (x)
        {
            case not null when y is null:
                return 1;
            case null when y is null:
                return 0;
            case null:
                return -1;
        }

        var p = x.SortingPriority - y.SortingPriority;
        return p == 0 ? StringComparer.InvariantCultureIgnoreCase.Compare(x.GroupName, y.GroupName) : p;
    }
}

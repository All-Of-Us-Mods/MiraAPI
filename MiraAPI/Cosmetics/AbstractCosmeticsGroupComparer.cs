using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiraAPI.Cosmetics;
internal class AbstractCosmeticsGroupComparer : Comparer<AbstractCosmeticsGroup> 
{
    public override int Compare(AbstractCosmeticsGroup x, AbstractCosmeticsGroup y)
    {
        var p = x.SortingPriority - y.SortingPriority;
        if (p == 0) return StringComparer.InvariantCultureIgnoreCase.Compare(x.GroupName, y.GroupName);
        return p;
    }
}

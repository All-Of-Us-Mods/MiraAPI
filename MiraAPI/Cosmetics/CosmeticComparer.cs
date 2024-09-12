using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Cosmetics;
internal class CosmeticComparer(AbstractCosmeticsGroup group) : Comparer<CosmeticData>
{
    internal static readonly string[] noCosmetics = ["hat_NoHat", "skin_None", "nameplate_NoPlate", "visor_EmptyVisor"];
    public override int Compare(CosmeticData x, CosmeticData y)
    {
        if (noCosmetics.Contains(x.ProdId)) return -1;
        if (noCosmetics.Contains(y.ProdId)) return 1;
        switch (group.SortingMode)
        {
            default:
            case SortingMode.AlphabeticName:
                return StringComparer.InvariantCultureIgnoreCase.Compare(x.GetItemName(), y.GetItemName());
            case SortingMode.AlphabeticProductId:
                return StringComparer.InvariantCultureIgnoreCase.Compare(x.ProductId, y.ProductId);
            case SortingMode.Priority:
                return CustomCosmeticManager._cosmeticToPriority[x] - CustomCosmeticManager._cosmeticToPriority[y];
            case SortingMode.AlphabeticStoreName:
                if (x is HatData h1 && y is HatData h2) return StringComparer.InvariantCultureIgnoreCase.Compare(h1.StoreName, h2.StoreName);
                if (x is SkinData s1 && y is SkinData s2) return StringComparer.InvariantCultureIgnoreCase.Compare(s1.StoreName, s2.StoreName);
                goto case SortingMode.AlphabeticName;
        }
    }
}

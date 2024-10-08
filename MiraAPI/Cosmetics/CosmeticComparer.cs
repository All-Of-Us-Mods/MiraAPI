using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.Cosmetics;
internal sealed class CosmeticComparer(AbstractCosmeticsGroup group) : Comparer<CosmeticData>
{
    internal static readonly string[] NoCosmetics = ["hat_NoHat", "skin_None", "nameplate_NoPlate", "visor_EmptyVisor"];
    public override int Compare(CosmeticData? x, CosmeticData? y)
    {
        if (NoCosmetics.Contains(x?.ProdId))
        {
            return -1;
        }

        if (NoCosmetics.Contains(y?.ProdId))
        {
            return 1;
        }

        return group.SortingMode switch
        {
            SortingMode.AlphabeticProductId => StringComparer.InvariantCultureIgnoreCase.Compare(x?.ProductId, y?.ProductId),
            SortingMode.Priority => CustomCosmeticManager.CosmeticToPriority[x!] - CustomCosmeticManager.CosmeticToPriority[y!],
            SortingMode.AlphabeticStoreName => x switch
            {
                HatData h1 when y is HatData h2 => StringComparer.InvariantCultureIgnoreCase.Compare(
                    h1.StoreName,
                    h2.StoreName),
                SkinData s1 when y is SkinData s2 => StringComparer.InvariantCultureIgnoreCase.Compare(
                    s1.StoreName,
                    s2.StoreName),
                _ => StringComparer.InvariantCultureIgnoreCase.Compare(x?.GetItemName(), y?.GetItemName()),
            },
            _ => StringComparer.InvariantCultureIgnoreCase.Compare(x?.GetItemName(), y?.GetItemName()),
        };
    }
}

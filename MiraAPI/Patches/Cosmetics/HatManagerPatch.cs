using HarmonyLib;
using MiraAPI.Cosmetics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiraAPI.Patches.Cosmetics;
[HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
public static class HatManagerPatch
{
    public static bool Prefix(HatManager __instance, string hatId, ref HatData __result)
    {
        __result = (HatData)CustomCosmeticManager._cosmeticToGroup.FirstOrDefault(x => string.Equals(x.Key.ProdId, hatId)).Key;
        if (__result) return false;
        return true;
    }
}

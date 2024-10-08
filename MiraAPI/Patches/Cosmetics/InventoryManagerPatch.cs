using HarmonyLib;
using MiraAPI.Cosmetics;

namespace MiraAPI.Patches.Cosmetics;

[HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.CheckUnlockedItems))]
public static class InventoryManagerPatch
{
    public static void Prefix()
    {
        CustomCosmeticManager.LoadAll();
    }
}

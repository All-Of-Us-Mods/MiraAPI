using HarmonyLib;
using MiraAPI.Cosmetics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiraAPI.Patches.Cosmetics;

[HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.CheckUnlockedItems))]
public static class InventoryManagerPatch
{
    public static void Prefix()
    {
        CustomCosmeticManager.LoadAll();
    }
}

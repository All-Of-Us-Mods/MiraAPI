/*
using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(NormalGameManager))]
internal static class NormalGameManagerPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(NormalGameManager.IsNormal))]
    public static void IsNormal(ref bool __result)
    {
        if (CustomGameModeManager.ActiveMode != null)
        {
            __result = !CustomGameModeManager.ActiveMode.IsHideAndSeek;
        }
    }
}
*/

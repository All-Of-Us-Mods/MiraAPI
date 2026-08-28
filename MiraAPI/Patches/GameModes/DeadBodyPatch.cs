using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(DeadBody))]
internal static class DeadBodyPatch
{
    [HarmonyPrefix, HarmonyPatch(nameof(DeadBody.OnClick))]
    public static bool OnClickPatch(DeadBody __instance)
    {
        return CustomGameModeManager.ActiveMode?.CanReport(__instance) == true;
    }
}

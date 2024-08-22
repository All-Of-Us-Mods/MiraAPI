using HarmonyLib;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(ExileController))]
public static class EjectionPatches
{
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ExileController.Begin))]
    public static void Begin(ExileController __instance)
    {
        if (!__instance.initData.networkedPlayer.Role || __instance.initData.networkedPlayer.Role is not ICustomRole role)
        {
            return;
        }

        if (role.GetCustomEjectionMessage(__instance.initData.networkedPlayer) == null)
        {
            return;
        }
        
        __instance.completeString = role.GetCustomEjectionMessage(__instance.initData.networkedPlayer);
    }
}
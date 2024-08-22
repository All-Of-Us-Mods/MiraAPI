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
        if (!__instance.exiled?.Role || __instance.exiled.Role is not ICustomRole role)
        {
            return;
        }

        if (role.GetCustomEjectionMessage(__instance.exiled) == null)
        {
            return;
        }
        
        __instance.completeString = role.GetCustomEjectionMessage(__instance.exiled);
    }
}
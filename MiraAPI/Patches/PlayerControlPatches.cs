using HarmonyLib;
using MiraAPI.Hud;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(PlayerControl))]
public static class PlayerControlPatches
{
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    public static void PlayerControlFixedUpdatePostfix(PlayerControl __instance)
    {
        if (!__instance.AmOwner) return;
        
        foreach (var button in CustomButtonManager.CustomButtons)
        {
            if (!button.Enabled(__instance.Data.Role))
            {
                continue;
            }
            
            button.FixedUpdateHandler(__instance);
        }
    }
    
}
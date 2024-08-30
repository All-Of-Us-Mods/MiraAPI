using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Roles;
using MiraAPI.Modifiers;
using Reactor.Utilities.Extensions;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(PlayerControl))]
public static class PlayerControlPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerControl.Start))]
    public static void PlayerControlStartPostfix(PlayerControl __instance)
    {
        if (__instance.gameObject.TryGetComponent<ModifierComponent>(out var comp))
        {
            comp.DestroyImmediate();
        }

        __instance.gameObject.AddComponent<ModifierComponent>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    public static void PlayerControlFixedUpdatePostfix(PlayerControl __instance)
    {
        if (__instance.Data?.Role is ICustomRole customRole)
        {
            customRole.PlayerControlFixedUpdate(__instance);
        }

        if (!__instance.AmOwner)
        {
            return;
        }

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            if (__instance.Data?.Role == null)
            {
                continue;
            }

            if (!button.Enabled(__instance.Data?.Role))
            {
                continue;
            }

            button.FixedUpdateHandler(__instance);
        }
    }
}

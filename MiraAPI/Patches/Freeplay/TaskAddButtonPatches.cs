using HarmonyLib;
using MiraAPI.Modifiers;

namespace MiraAPI.Patches.Freeplay;

[HarmonyPatch(typeof(TaskAddButton))]
internal static class TaskAddButtonPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaskAddButton.Start))]
    public static bool StartPrefix(TaskAddButton __instance)
    {
        // if this becomes problematic in the future, find a new method.
        if (!uint.TryParse(__instance.name, out var result))
            return true;

        __instance.Overlay.sprite = __instance.CheckImage;
        __instance.Overlay.enabled = PlayerControl.LocalPlayer.HasModifier(result);
        return false;
    }
}

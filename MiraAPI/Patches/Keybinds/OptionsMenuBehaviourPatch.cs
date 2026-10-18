using System.Linq;
using HarmonyLib;
using MiraAPI.Keybinds;
using UnityEngine;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuBehaviourPatch
{
    private static ButtonRolloverHandler? _remapRollover;
    private static SpriteRenderer? _remapBackground;

    private static bool Conflicts => KeybindManager.GetConflicts().Count > 0 || KeybindManager.GetControllerConflicts().Count > 0;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Open))]
    private static void OpenPostfix(OptionsMenuBehaviour __instance)
    {
        _remapRollover = __instance.GetComponentsInChildren<ButtonRolloverHandler>(true).FirstOrDefault(x => x.name == "Remap Controls");
        _remapBackground = _remapRollover?.transform.FindChild("Background")?.GetComponent<SpriteRenderer>();
        if (_remapBackground != null) _remapBackground.color = Conflicts ? Color.red : Color.white;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Update))]
    private static bool UpdatePrefix(OptionsMenuBehaviour __instance)
    {
        if (MiraApiPlugin.IsMobile || ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick) return true;

        if (Input.GetKeyUp(KeyCode.Escape) && !__instance.KeyboardOptions.activeSelf) __instance.Close();

        __instance.KeyboardOptions.SetActive(true);
        __instance.MouseAndKeyboardOptions.SetActive(true);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Update))]
    private static void UpdatePostfix()
    {
        if (_remapRollover == null) return;

        if (_remapBackground == null) return;

        var conflicts = Conflicts;
        _remapRollover.OutColor = conflicts ? Color.red : Color.white;
        _remapRollover.UnselectedColor = conflicts ? Color.red : Color.white;
        _remapRollover.OverColor = conflicts ? new Color32(255, 55, 55, 255) : Palette.AcceptedGreen;
        if (!conflicts && _remapBackground.color == Color.red) _remapBackground.color = Color.white;
    }
}

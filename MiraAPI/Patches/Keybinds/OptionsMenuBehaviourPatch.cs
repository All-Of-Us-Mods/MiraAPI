using HarmonyLib;
using MiraAPI.Keybinds;
using UnityEngine;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuBehaviourPatch
{
    private static ButtonRolloverHandler? _remap_rollover;
    private static SpriteRenderer? _remap_background;
    private static bool _conflicts => KeybindManager.GetConflicts().Count > 0;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Open))]
    private static void OpenPostfix(OptionsMenuBehaviour __instance)
    {
        _remap_rollover = GameObject.Find("Remap Controls")?.GetComponent<ButtonRolloverHandler>()!;
        _remap_background = _remap_rollover.transform.FindChild("Background").GetComponent<SpriteRenderer>();
        _remap_background.color = _conflicts ? Color.red : Color.white;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Update))]
    private static void UpdatePostfix(OptionsMenuBehaviour __instance)
    {
        if (_remap_rollover == null)
        {
            return;
        }
        if (_remap_background == null)
        {
            return;
        }

        _remap_rollover.OutColor = _conflicts ? Color.red : Color.white;
        _remap_rollover.UnselectedColor = _conflicts ? Color.red : Color.white;
        _remap_rollover.OverColor = _conflicts ? new Color32(255, 55, 55, 255) : Palette.AcceptedGreen;
    }
}

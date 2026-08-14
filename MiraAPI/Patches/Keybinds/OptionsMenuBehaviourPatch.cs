using HarmonyLib;
using MiraAPI.Keybinds;
using UnityEngine;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuBehaviourPatch
{
    private static ButtonRolloverHandler? _remap_rollover;
    private static SpriteRenderer? _remap_background;

    private static bool Conflicts => KeybindManager.GetConflicts().Count > 0;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Open))]
    private static void OpenPostfix()
    {
        _remap_rollover = GameObject.Find("Remap Controls")?.GetComponent<ButtonRolloverHandler>()!;
        try
        {
            _remap_background = _remap_rollover.transform.FindChild("Background").GetComponent<SpriteRenderer>();
            _remap_background.color = Conflicts ? Color.red : Color.white;
        }
        catch
        {
            // ignored, this normally breaks when on mobile since there's no remap button screen.
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Update))]
    private static void UpdatePostfix()
    {
        if (_remap_rollover == null)
        {
            return;
        }
        if (_remap_background == null)
        {
            return;
        }

        _remap_rollover.OutColor = Conflicts ? Color.red : Color.white;
        _remap_rollover.UnselectedColor = Conflicts ? Color.red : Color.white;
        _remap_rollover.OverColor = Conflicts ? new Color32(255, 55, 55, 255) : Palette.AcceptedGreen;
    }
}

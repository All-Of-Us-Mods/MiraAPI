using HarmonyLib;
using MiraAPI.Keybinds;
using UnityEngine;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuBehaviourPatch
{
    private static ButtonRolloverHandler? _remapRollover;
    private static SpriteRenderer? _remapBackground;

    private static bool Conflicts => KeybindManager.GetConflicts().Count > 0;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Open))]
    private static void OpenPostfix()
    {
        _remapRollover = GameObject.Find("Remap Controls")?.GetComponent<ButtonRolloverHandler>()!;
        try
        {
            _remapBackground = _remapRollover.transform.FindChild("Background").GetComponent<SpriteRenderer>();
            _remapBackground.color = Conflicts ? Color.red : Color.white;
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
        if (_remapRollover == null)
        {
            return;
        }
        if (_remapBackground == null)
        {
            return;
        }

        _remapRollover.OutColor = Conflicts ? Color.red : Color.white;
        _remapRollover.UnselectedColor = Conflicts ? Color.red : Color.white;
        _remapRollover.OverColor = Conflicts ? new Color32(255, 55, 55, 255) : Palette.AcceptedGreen;
    }
}

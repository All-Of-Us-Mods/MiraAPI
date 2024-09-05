using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches;

/// <summary>
/// General patches for the HudManager class.
/// </summary>
[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    // Custom buttons parent.
    private static GameObject? _bottomLeft;

    /*
    /// <summary>
    /// Trigger hudstart on current custom gamemode
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.OnGameStart))]
    public static void GameStartPatch(HudManager __instance)
    {
        CustomGameModeManager.ActiveMode?.HudStart(__instance);
    }*/

    /// <summary>
    /// Create custom buttons and arrange them on the hud.
    /// </summary>
    /// <param name="__instance">The HudManager instance.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Start))]
    public static void StartPostfix(HudManager __instance)
    {
        var buttons = __instance.transform.Find("Buttons");
        var bottomRight = buttons.Find("BottomRight");

        if (_bottomLeft == null)
        {
            _bottomLeft = Object.Instantiate(bottomRight.gameObject, buttons);
        }

        foreach (var t in _bottomLeft.GetComponentsInChildren<ActionButton>(true))
        {
            t.gameObject.Destroy();
        }

        var gridArrange = _bottomLeft.GetComponent<GridArrange>();
        var aspectPosition = _bottomLeft.GetComponent<AspectPosition>();

        _bottomLeft.name = "BottomLeft";
        gridArrange.Alignment = GridArrange.StartAlign.Right;
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            var location = button.Location switch
            {
                ButtonLocation.BottomLeft => _bottomLeft.transform,
                ButtonLocation.BottomRight => bottomRight,
                _ => null,
            };

            if (location is null)
            {
                continue;
            }

            button.CreateButton(location);
        }

        gridArrange.Start();
        gridArrange.ArrangeChilds();

        aspectPosition.AdjustPosition();
    }

    /// <summary>
    /// Set the custom buttons active when the hud is active.
    /// </summary>
    /// <param name="__instance">HudManager instance.</param>
    /// <param name="localPlayer">The local PlayerControl.</param>
    /// <param name="role">The player's RoleBehaviour.</param>
    /// <param name="isActive">Whether the Hud should be set active or not.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void SetHudActivePostfix(HudManager __instance, PlayerControl localPlayer, RoleBehaviour role, bool isActive)
    {
        if (!localPlayer.Data)
        {
            return;
        }

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            button.SetActive(isActive, role);
        }
    }
}

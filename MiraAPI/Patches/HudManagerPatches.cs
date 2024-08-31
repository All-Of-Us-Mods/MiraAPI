using HarmonyLib;
using InnerNet;
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

    // Custom role tab.
    private static TaskPanelBehaviour? _roleTab;

    /// <summary>
    /// Update custom role tab and custom role hud elements.
    /// </summary>
    /// <param name="__instance">The HudManager instance.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Update))]
    public static void UpdatePostfix(HudManager __instance)
    {
        var local = PlayerControl.LocalPlayer;

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !ShipStatus.Instance)
        {
            return;
        }

        // CustomGameModeManager.ActiveMode?.HudUpdate(__instance);

        switch (local?.Data?.Role)
        {
            case null:
                return;

            case ICustomRole customRole:
            {
                customRole.HudUpdate(__instance);

                if (customRole.RoleHintType != RoleHintType.RoleTab)
                {
                    _roleTab?.gameObject.Destroy();
                    return;
                }

                if (_roleTab == null)
                {
                    _roleTab = CustomRoleManager.CreateRoleTab(customRole);
                }
                else
                {
                    CustomRoleManager.UpdateRoleTab(_roleTab, customRole);
                }

                break;
            }

            default:
                if (_roleTab != null)
                {
                    _roleTab.gameObject.Destroy();
                }
                break;
        }
    }

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
    /// Set the custom role tab and custom buttons active when the hud is active.
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

        _roleTab?.gameObject.SetActive(isActive);

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            button.SetActive(isActive, role);
        }

        if (role is ICustomRole customRole)
        {
            __instance.ImpostorVentButton.gameObject.SetActive(isActive && customRole.CanUseVent);
        }
    }
}

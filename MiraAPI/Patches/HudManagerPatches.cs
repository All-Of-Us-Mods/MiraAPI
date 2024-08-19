using HarmonyLib;
using InnerNet;
using MiraAPI.Hud;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    /// Custom buttons parent
    private static GameObject _bottomLeft;

    /// Custom role tab
    private static TaskPanelBehaviour _roleTab;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Update))]
    public static void UpdatePostfix(HudManager __instance)
    {
        var local = PlayerControl.LocalPlayer;

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !ShipStatus.Instance)
        {
            return;
        }

        //CustomGameModeManager.ActiveMode.HudUpdate(__instance);
        if (local is null || local.Data is null || local.Data.Role is null) return;

        if (local.Data.Role is ICustomRole customRole)
        {
            customRole.HudUpdate(__instance);

            if (customRole.SetTabText() != null)
            {
                if (_roleTab == null)
                {
                    _roleTab = CustomRoleManager.CreateRoleTab(customRole);
                }
                else
                {
                    CustomRoleManager.UpdateRoleTab(_roleTab, customRole);
                }
            }
            else if (customRole.SetTabText() == null && _roleTab)
            {
                _roleTab.gameObject.Destroy();
            }
        }
        else if (_roleTab)
        {
            _roleTab.gameObject.Destroy();
        }
    }

    /*        /// <summary>
            /// Trigger hudstart on current custom gamemode
            /// </summary>
            [HarmonyPostfix]
            [HarmonyPatch("OnGameStart")]
            public static void GameStartPatch(HudManager __instance)
            {
                CustomGameModeManager.ActiveMode.HudStart(__instance);
            }*/

    /// <summary>
    /// Create custom buttons parent
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Start))]
    public static void StartPostfix(HudManager __instance)
    {
        if (!_bottomLeft)
        {
            var buttons = __instance.transform.Find("Buttons");
            _bottomLeft = Object.Instantiate(buttons.Find("BottomRight").gameObject, buttons);
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
            button.CreateButton(_bottomLeft.transform);
        }

        gridArrange.Start();
        gridArrange.ArrangeChilds();

        aspectPosition.AdjustPosition();
    }

    /// <summary>
    /// Make sure all launchpad hud elements are inactive/active when appropriate
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void SetHudActivePostfix(HudManager __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] RoleBehaviour roleBehaviour, [HarmonyArgument(2)] bool isActive)
    {
        if (player.Data == null)
        {
            return;
        }

        if (_roleTab)
        {
            _roleTab.gameObject.SetActive(isActive);
        }

        foreach (var button in CustomButtonManager.CustomButtons)
            button.SetActive(isActive, roleBehaviour);

        if (roleBehaviour is ICustomRole role)
        {
            __instance.ImpostorVentButton.gameObject.SetActive(isActive && role.CanUseVent);
        }
    }
}
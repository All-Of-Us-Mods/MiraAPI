using HarmonyLib;
using InnerNet;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// HudManager patches for roles.
/// </summary>
[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    // Custom role tab.
    private static TaskPanelBehaviour? _roleTab;

    /// <summary>
    /// Fixes Kill Button not showing for Neutral killing role.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void SetHudActivePostfix(
        HudManager __instance,
        PlayerControl localPlayer,
        RoleBehaviour role,
        bool isActive)
    {
        var flag = localPlayer.Data != null && localPlayer.Data.IsDead;

        if (role is ICustomRole)
        {
            __instance.KillButton.ToggleVisible(isActive && role.CanUseKillButton && !flag);
            __instance.ImpostorVentButton.ToggleVisible(isActive && role.CanVent && !flag);
        }

        if (_roleTab)
        {
            _roleTab?.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Update custom role tab and custom role hud elements.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Update))]
    public static void UpdatePostfix(HudManager __instance)
    {
        var local = PlayerControl.LocalPlayer;

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !ShipStatus.Instance)
        {
            return;
        }

        var role = local?.Data?.Role;

        if (role is ICustomRole { RoleHintType: RoleHintType.RoleTab } customRole)
        {
            customRole.HudUpdate(__instance);

            if (_roleTab == null)
            {
                _roleTab = CustomRoleManager.CreateRoleTab(customRole);
            }

            CustomRoleManager.UpdateRoleTab(_roleTab, customRole);
        }
        else if (_roleTab)
        {
            _roleTab?.gameObject.Destroy();
        }
    }
}

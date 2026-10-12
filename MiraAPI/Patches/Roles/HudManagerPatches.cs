using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using InnerNet;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// <see cref="HudManager"/> patches for roles.
/// </summary>
[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    // Custom role tab.
    [SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Internal behaviour that does not need property-level validation.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
    [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Read above.")]
    [SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Read above.")] // why so many warnings???
    public static TaskPanelBehaviour? RoleTab;

    /// <summary>
    /// Fixes <see cref="HudManager.KillButton"/> not showing for Neutral killing role.
    /// </summary>
    /// <param name="__instance"><see cref="HudManager"/> instance.</param>
    /// <param name="localPlayer">The local <see cref="PlayerControl"/>.</param>
    /// <param name="role">The player's <see cref="RoleBehaviour"/>.</param>
    /// <param name="isActive">Whether the Hud should be set active or not.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void SetHudActivePostfix(
        HudManager __instance,
        PlayerControl localPlayer,
        RoleBehaviour role,
        bool isActive)
    {
        var flag = localPlayer.Data != null && localPlayer.Data.IsDead;

        if (role is ICustomRole customRole)
        {
            __instance.KillButton.ToggleVisible(isActive && customRole.Configuration.UseVanillaKillButton && !flag);
            __instance.ImpostorVentButton.ToggleVisible(isActive && customRole.Configuration.CanUseVent && !flag);
            __instance.SabotageButton.gameObject.SetActive(isActive && customRole.Configuration.CanUseSabotage);
        }

        if (RoleTab)
        {
            RoleTab?.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Update custom role tab and custom role hud elements.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Update))]
    public static void UpdatePostfix()
    {
        var local = PlayerControl.LocalPlayer;

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && !ShipStatus.Instance)
        {
            return;
        }

        var role = local?.Data?.Role;

        if (role is ICustomRole { Configuration.RoleHintType: RoleHintType.RoleTab } customRole)
        {
            if (RoleTab == null)
            {
                RoleTab = CustomRoleManager.CreateRoleTab(customRole);
            }

            CustomRoleManager.UpdateRoleTab(RoleTab, customRole);
        }
        else if (RoleTab != null)
        {
            RoleTab.gameObject.Destroy();
            RoleTab = null;
        }
    }
}

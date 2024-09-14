using HarmonyLib;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(SabotageButton))]
public class SabotageButtonPatch
{
    /// <summary>
    /// Patches the Sabotage button to check if the player's custom role can use sabotage.
    /// </summary>
    [HarmonyPatch(nameof(SabotageButton.DoClick))]
    [HarmonyPrefix]
    public static bool DoClickPrefix(SabotageButton __instance)
    {
        var player = PlayerControl.LocalPlayer;

        if (player.Data.Role is not ICustomRole customRole)
        {
            return true;
        }

        if (!customRole.Configuration.CanUseSabotage || PlayerControl.LocalPlayer.inVent || !GameManager.Instance.SabotagesEnabled())
        {
            return false;
        }

        HudManager.Instance.ToggleMapVisible(
            new MapOptions
            {
                Mode = MapOptions.Modes.Sabotage,
            });
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SabotageButton.Refresh))]
    public static bool RefreshPrefix(SabotageButton __instance)
    {
        var player = PlayerControl.LocalPlayer;
        if (GameManager.Instance == null || player == null)
        {
            __instance.ToggleVisible(false);
            __instance.SetDisabled();
            return false;
        }

        if (player.Data.Role is not ICustomRole customRole)
        {
            return true;
        }

        if (player.inVent || !GameManager.Instance.SabotagesEnabled() || player.petting)
        {
            __instance.ToggleVisible(customRole.Configuration.CanUseSabotage && GameManager.Instance.SabotagesEnabled());
            __instance.SetDisabled();
            return false;
        }
        __instance.SetEnabled();
        return false;
    }
}

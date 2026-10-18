using HarmonyLib;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Set nametag color depending on visibility.
/// </summary>
[HarmonyPatch(typeof(PlayerNameColor))]
public static class NameTagPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerNameColor.Get), typeof(RoleBehaviour))]
    public static bool GetPatch([HarmonyArgument(0)] RoleBehaviour otherPlayerRole, ref Color __result)
    {
        if (otherPlayerRole is ICustomRole customRole && customRole.CanLocalPlayerSeeRole(otherPlayerRole.Player))
        {
            __result = customRole.RoleColor;
            return false;
        }

        if (PlayerControl.LocalPlayer.Data?.Role?.IsImpostor == true && otherPlayerRole.IsImpostor)
        {
            return true;
        }

        if (GameManager.Instance.IsHideAndSeek())
        {
            return true;
        }

        if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.Data || !PlayerControl.LocalPlayer.Data?.Role || !otherPlayerRole)
        {
            __result = Color.white;
            return false;
        }

        __result = PlayerControl.LocalPlayer.Data?.Role == otherPlayerRole ? otherPlayerRole.NameColor : Color.white;
        return false;
    }
}

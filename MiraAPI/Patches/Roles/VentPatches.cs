using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Vent patches for Roles.
/// </summary>
[HarmonyPatch(typeof(Vent))]
public static class VentPatches
{
    /// <summary>
    /// Set CanVent of player to customRole CanUseVent because the original one isnt a property :(.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Vent.CanUse))]
    public static void CanUsePrefix(Vent __instance, NetworkedPlayerInfo pc)
    {
        if (pc.Role is ICustomRole customRole)
        {
            pc.Role.CanVent = customRole.CanUseVent;
        }
    }

    /// <summary>
    /// Set outline to player's custom role color.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Vent.SetOutline))]
    public static void SetOutlinePostfix(Vent __instance, bool on, bool mainTarget)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole)
        {
            return;
        }

        var color = customRole.RoleColor;

        __instance.myRend.material.SetFloat(ShaderID.Outline, on ? 1 : 0);
        __instance.myRend.material.SetColor(ShaderID.OutlineColor, color);
        __instance.myRend.material.SetColor(ShaderID.AddColor, mainTarget ? color : Color.clear);
    }
}

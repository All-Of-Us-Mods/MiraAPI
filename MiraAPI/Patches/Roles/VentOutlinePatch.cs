using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Set vent outline color based on the player's role.
/// </summary>
[HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
public static class VentOutlinePatch
{
    public static void Postfix(Vent __instance, bool on, bool mainTarget)
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

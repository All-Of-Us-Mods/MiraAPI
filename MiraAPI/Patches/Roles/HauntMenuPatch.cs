using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Patches the <see cref="HauntMenuMinigame"/> to show the actual role name rather than team.
/// </summary>
// SetFilterText was INLINED
[HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.SetHauntTarget))]
public static class HauntMenuMinigamePatch
{
    public static void Postfix(HauntMenuMinigame __instance)
    {
        if (__instance.HauntTarget.Data.IsDead)
        {
            __instance.FilterText.color = Color.white;
            return;
        }

        var role = __instance.HauntTarget.Data.Role;
        var color = role is ICustomRole custom ? custom.RoleColor : role.TeamColor;

        __instance.FilterText.text = role.GetRoleName();
        __instance.FilterText.color = color;
    }
}

using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using System.Linq;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(RoleManager))]
public static class RoleManagerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(RoleManager.SelectRoles))]
    public static void ModifierSelectionPatches(RoleManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        ModifierManager.AssignModifiers(PlayerControl.AllPlayerControls.ToArray().Where(plr => !plr.Data.IsDead && !plr.Data.Disconnected).ToList());
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleManager.AssignRoleOnDeath))]
    public static bool AssignRoleOnDeath(RoleManager __instance, [HarmonyArgument(0)] PlayerControl plr)
    {
        if (!plr || !plr.Data.IsDead)
        {
            return false;
        }

        if (plr.Data.Role is not ICustomRole role)
        {
            return true;
        }

        if (role.GhostRole is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost)
        {
            return true;
        }
        
        plr.RpcSetRole(role.GhostRole);
        return false;

    }
}
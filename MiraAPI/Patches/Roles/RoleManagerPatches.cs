using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(RoleManager))]
public static class RoleManagerPatches
{
    [HarmonyPrefix, HarmonyPatch("AssignRoleOnDeath")]
    public static bool AssignRoleOnDeath(RoleManager __instance, [HarmonyArgument(0)] PlayerControl plr)
    {
        if (plr == null || !plr.Data.IsDead)
        {
            return false;
        }

        if (plr.Data.Role is ICustomRole role)
        {
            Debug.Log(role.RoleName);
            if (role.GhostRole != RoleTypes.CrewmateGhost && role.GhostRole != RoleTypes.ImpostorGhost)
            {
                plr.RpcSetRole(role.GhostRole);
                return false;
            }
        }

        return true;
    }
}
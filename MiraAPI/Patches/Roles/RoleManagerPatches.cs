using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(RoleManager))]
public static class RoleManagerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleManager.SetRole))]
    // NOTE: As of 2025.5.20 update, this is INLINED in ONE place: GameManager::ReviveEveryoneFreeplay(void).
    // Should not affect normal gameplay, but may cause issues in Freeplay.
    public static bool SetRolePatch(RoleManager __instance, PlayerControl targetPlayer, RoleTypes roleType)
    {
        if (!targetPlayer)
        {
            return false;
        }
        var data = targetPlayer.Data;
        if (data == null)
        {
            Error("It shouldn't be possible, but " + targetPlayer.name + " still doesn't have PlayerData during role selection.");
            return false;
        }
        if (data.Role)
        {
            data.Role.Deinitialize(targetPlayer);
            Object.Destroy(data.Role.gameObject);
        }
        var roleBehaviour = Object.Instantiate<RoleBehaviour>(__instance.AllRoles.ToArray().First(r => r.Role == roleType), data.gameObject.transform);
        roleBehaviour.Initialize(targetPlayer);
        targetPlayer.Data.Role = roleBehaviour;
        targetPlayer.Data.RoleType = roleType;

        // basically everything is the same except this one if statement
        // innersloth decided to check the roleType manually instead of the role behaviour.
        if (!roleBehaviour.IsDead)
        {
            targetPlayer.Data.RoleWhenAlive = new Il2CppSystem.Nullable<RoleTypes>(roleType);
        }
        roleBehaviour.AdjustTasks(targetPlayer);
        switch (roleBehaviour.IsDead)
        {
            case true when !targetPlayer.Data.IsDead:
                targetPlayer.Die(DeathReason.Kill, false);
                return false;
            case false when targetPlayer.Data.IsDead:
                targetPlayer.Revive();
                break;
        }

        var @event = new SetRoleEvent(targetPlayer, roleType);
        MiraEventManager.InvokeEvent(@event);

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RoleManager.SelectRoles))]
    public static void ModifierSelectionPatches()
    {
        if (!AmongUsClient.Instance.AmHost || !ModifierManager.MiraAssignsModifiers)
        {
            return;
        }

        ModifierManager.AssignModifiers([.. PlayerControl.AllPlayerControls.ToArray().Where(plr => !plr.Data.IsDead && !plr.Data.Disconnected)]);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleManager.AssignRoleOnDeath))]
    public static bool AssignRoleOnDeath([HarmonyArgument(0)] PlayerControl plr)
    {
        if (!plr || !plr.Data.IsDead)
        {
            return false;
        }

        if (plr.Data.Role is not ICustomRole role)
        {
            return true;
        }

        if (role.Configuration.GhostRole is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost)
        {
            return true;
        }

        plr.RpcSetRole(role.Configuration.GhostRole);
        return false;
    }
}

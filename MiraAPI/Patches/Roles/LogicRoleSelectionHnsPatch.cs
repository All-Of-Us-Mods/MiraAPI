using System;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(LogicRoleSelectionHnS))]
public static class LogicRoleSelectionHnsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LogicRoleSelectionHnS.AssignRolesForTeam))]
    public static bool AssignRolesForTeam(
        LogicRoleSelectionHnS __instance,
        List<NetworkedPlayerInfo> players,
        IGameOptions opts,
        RoleTeamTypes team,
        int teamMax,
        Il2CppSystem.Nullable<RoleTypes> defaultRole)
    {
        Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Team: {team}, Max: {teamMax}, Players: {players.Count}, DefaultRole: {defaultRole}");
        var num = 0;
        var roleOptions = opts.RoleOptions;

        var source = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => role.TeamType == team && !RoleManager.IsGhostRole(role.Role) &&
                           CustomRoleUtils.CanSpawnOnCurrentMode(role)).ToArray();

        var assignmentData = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        var source2 = CustomRoleUtils.GetPossibleRoles(assignmentData, x => x.Chance == 100);
        var guaranteedRoles = source.Where(x => source2.Contains(((ushort)x.Role, 100)));
        var list = new List<RoleTypes>();

        switch (team)
        {
            case RoleTeamTypes.Crewmate:
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
                AddGuaranteedRoles(guaranteedRoles, opts, list);
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");
                AssignRolesFromList(players, teamMax, list, ref num);

                AddPotentialRoles(source, opts, list);
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Potential Assignment");
                AssignRolesFromList(players, teamMax, list, ref num);

                const RoleTypes basicCrewRole = RoleTypes.Engineer;
                AddFallbackRoles(list, players.Count, teamMax, num, basicCrewRole);
                AssignRolesFromList(players, teamMax, list, ref num);
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Fallback Assignment");
                break;

            case RoleTeamTypes.Impostor:
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
                var newImpostors = new List<NetworkedPlayerInfo>();

                // Specified Seeker
                if (__instance.hnsManager.LogicOptionsHnS.HasImpostorPlayerID() &&
                    __instance.hnsManager.LogicOptionsHnS.ValidateImpostorPlayerID(players) &&
                    !AmongUsClient.Instance.IsGamePublic)
                {
                    var networkedPlayerInfo = players.ToArray()
                        .First(p => p.PlayerId == __instance.hnsManager.LogicOptionsHnS.ImpostorPlayerID());
                    players.Remove(networkedPlayerInfo);
                    newImpostors.Add(networkedPlayerInfo);
                    Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
                }
                // Random Seeker
                else
                {
                    var num2 = 0;
                    while (num2 < teamMax && players.Count > 0)
                    {
                        var pseudoRandomList = new PseudoRandomList<NetworkedPlayerInfo>(AmongUsClient.Instance.GameId);
                        players._items.Do(pseudoRandomList.Add);
                        for (var i = 0; i < GameData.RoundsPlayedInSession; i++)
                        {
                            pseudoRandomList.PickRandom();
                        }
                        var networkedPlayerInfo = pseudoRandomList.PickRandom();
                        players.Remove(networkedPlayerInfo);
                        newImpostors.Add(networkedPlayerInfo);
                        num2++;
                        Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
                    }
                }
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");

                AddGuaranteedRoles(guaranteedRoles, opts, list);
                AssignRolesFromList(newImpostors, teamMax, list, ref num);

                AddPotentialRoles(source, opts, list);
                AssignRolesFromList(newImpostors, teamMax, list, ref num);

                const RoleTypes basicImpRole = RoleTypes.Impostor;
                AddFallbackRoles(list, newImpostors.Count, teamMax, num, basicImpRole);
                AssignRolesFromList(newImpostors, teamMax, list, ref num);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(team), team, null);
        }
        return false;
    }

    public static void AssignRolesFromList(List<NetworkedPlayerInfo> players, int teamMax, List<RoleTypes> roleList, ref int rolesAssigned)
    {
        while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
        {
            var index = HashRandom.FastNext(roleList.Count);
            var roleType = roleList[index];
            roleList.RemoveAt(index);
            var index2 = HashRandom.FastNext(players.Count);
            players[index2].Object.RpcSetRole(roleType);
            players.RemoveAt(index2);
            rolesAssigned++;
        }
    }

    private static void AddGuaranteedRoles(
        System.Collections.Generic.IEnumerable<RoleBehaviour> guaranteedRoles,
        IGameOptions opts,
        List<RoleTypes> list)
    {
        var roleOptions = opts.RoleOptions;
        foreach (var roleAssignmentData in guaranteedRoles.Select(x =>
                     new RoleManager.RoleAssignmentData(x, roleOptions.GetNumPerGame(x.Role), 100)))
        {
            while (true)
            {
                var count = roleAssignmentData.Count;
                roleAssignmentData.Count = count - 1;
                if (count <= 0) break;
                list.Add(roleAssignmentData.Role.Role);
            }
        }
    }

    private static void AddPotentialRoles(System.Collections.Generic.IEnumerable<RoleBehaviour> source, IGameOptions opts, List<RoleTypes> list)
    {
        var roleOptions = opts.RoleOptions;
        var potentialRoles = source.Where(x => !x.IsDead).Select(role => new RoleManager.RoleAssignmentData(
            role,
            roleOptions.GetNumPerGame(role.Role),
            roleOptions.GetChancePerGame(role.Role))).ToList();

        list.Clear();
        foreach (var roleData in potentialRoles)
        {
            for (var i = 0; i < roleData.Count; i++)
            {
                if (HashRandom.Next(101) < roleData.Chance)
                {
                    list.Add(roleData.Role.Role);
                }
            }
        }
    }

    private static void AddFallbackRoles(List<RoleTypes> list, int targetPlayerCount, int teamMax, int rolesAssigned, RoleTypes basicRole)
    {
        while (list.Count < targetPlayerCount && list.Count + rolesAssigned < teamMax)
        {
            list.Add(basicRole);
        }
    }
}

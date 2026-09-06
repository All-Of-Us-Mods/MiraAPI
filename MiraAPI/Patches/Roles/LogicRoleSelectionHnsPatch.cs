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
        Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Team: {team}, Max: {teamMax}, Players: {players.Count}, DefaultRole: {defaultRole}");
        int num = 0;
        IRoleOptionsCollection roleOptions = opts.RoleOptions;
        var source = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => role.TeamType == team && !RoleManager.IsGhostRole(role.Role) &&
                           CustomRoleUtils.CanSpawnOnCurrentMode(role));
        var assignmentData = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();
        var source2 = CustomRoleUtils.GetPossibleRoles(assignmentData, x => x.Chance == 100);
        var guaranteedRoles = source.Where(x => source2.Contains(((ushort)x.Role, 100)));
        var list = new List<RoleTypes>();
        if (team == RoleTeamTypes.Crewmate)
        {
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
            foreach (RoleManager.RoleAssignmentData roleAssignmentData in guaranteedRoles.Select((x) =>
                         new RoleManager.RoleAssignmentData(x, roleOptions.GetNumPerGame(x.Role), 100)))
            {
                while (true)
                {
                    RoleManager.RoleAssignmentData roleAssignmentData2 = roleAssignmentData;
                    int count = roleAssignmentData2.Count;
                    roleAssignmentData2.Count = count - 1;
                    if (count <= 0)
                    {
                        break;
                    }

                    list.Add(roleAssignmentData.Role.Role);
                }
            }
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");
            AssignRolesFromList(players, teamMax, list, ref num);

            var list2 = source.Where(x => !x.IsDead).Select(role =>
                new RoleManager.RoleAssignmentData(
                    role,
                    roleOptions.GetNumPerGame(role.Role),
                    roleOptions.GetChancePerGame(role.Role))).ToList();

            list.Clear();
            foreach (RoleManager.RoleAssignmentData roleAssignmentData3 in list2)
            {
                for (int i = 0; i < roleAssignmentData3.Count; i++)
                {
                    if (HashRandom.Next(101) < roleAssignmentData3.Chance)
                    {
                        list.Add(roleAssignmentData3.Role.Role);
                    }
                }
            }
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Potential Assignment");

            AssignRolesFromList(players, teamMax, list, ref num);
            var basicRole = RoleTypes.Engineer;
            while (list.Count < players.Count && list.Count + num < teamMax)
            {
                list.Add(basicRole);
            }

            AssignRolesFromList(players, teamMax, list, ref num);
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Fallback Assignment");
        }
        else if (team == RoleTeamTypes.Impostor)
        {
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
            var newImpostors = new List<NetworkedPlayerInfo>();
            // Specified Seeker
            if (__instance.hnsManager.LogicOptionsHnS.HasImpostorPlayerID() &&
                __instance.hnsManager.LogicOptionsHnS.ValidateImpostorPlayerID(players) &&
                !AmongUsClient.Instance.IsGamePublic)
            {
                NetworkedPlayerInfo networkedPlayerInfo = players.ToArray()
                    .First(p => p.PlayerId == __instance.hnsManager.LogicOptionsHnS.ImpostorPlayerID());
                players.Remove(networkedPlayerInfo);
                newImpostors.Add(networkedPlayerInfo);
                Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
            }
            // Random Seeker
            else
            {
                int num2 = 0;
                while (num2 < teamMax && players.Count > 0)
                {
                    var pseudoRandomList = new PseudoRandomList<NetworkedPlayerInfo>(AmongUsClient.Instance.GameId);
                    players._items.Do(pseudoRandomList.Add);
                    for (int i = 0; i < GameData.RoundsPlayedInSession; i++)
                    {
                        pseudoRandomList.PickRandom();
                    }
                    NetworkedPlayerInfo networkedPlayerInfo = pseudoRandomList.PickRandom();
                    players.Remove(networkedPlayerInfo);
                    newImpostors.Add(networkedPlayerInfo);
                    num2++;
                    Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
                }
            }
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");
            foreach (RoleManager.RoleAssignmentData roleAssignmentData in guaranteedRoles.Select((x) =>
                         new RoleManager.RoleAssignmentData(x, roleOptions.GetNumPerGame(x.Role), 100)))
            {
                while (true)
                {
                    RoleManager.RoleAssignmentData roleAssignmentData2 = roleAssignmentData;
                    int count = roleAssignmentData2.Count;
                    roleAssignmentData2.Count = count - 1;
                    if (count <= 0)
                    {
                        break;
                    }

                    list.Add(roleAssignmentData.Role.Role);
                }
            }
            AssignRolesFromList(newImpostors, teamMax, list, ref num);

            var list2 = source.Where(x => !x.IsDead).Select(role =>
                new RoleManager.RoleAssignmentData(
                    role,
                    roleOptions.GetNumPerGame(role.Role),
                    roleOptions.GetChancePerGame(role.Role))).ToList();

            list.Clear();
            foreach (RoleManager.RoleAssignmentData roleAssignmentData3 in list2)
            {
                for (int i = 0; i < roleAssignmentData3.Count; i++)
                {
                    if (HashRandom.Next(101) < roleAssignmentData3.Chance)
                    {
                        list.Add(roleAssignmentData3.Role.Role);
                    }
                }
            }

            AssignRolesFromList(newImpostors, teamMax, list, ref num);
            var basicRole = RoleTypes.Impostor;
            while (list.Count < newImpostors.Count && list.Count + num < teamMax)
            {
                list.Add(basicRole);
            }

            AssignRolesFromList(newImpostors, teamMax, list, ref num);
        }
        return false;
    }

    public static void AssignRolesFromList(List<NetworkedPlayerInfo> players, int teamMax, List<RoleTypes> roleList, ref int rolesAssigned)
    {
        while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
        {
            int index = HashRandom.FastNext(roleList.Count);
            RoleTypes roleType = roleList[index];
            roleList.RemoveAt(index);
            int index2 = HashRandom.FastNext(players.Count);
            players[index2].Object.RpcSetRole(roleType);
            players.RemoveAt(index2);
            rolesAssigned++;
        }
    }
}

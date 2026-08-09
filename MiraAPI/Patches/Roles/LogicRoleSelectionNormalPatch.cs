using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(LogicRoleSelectionNormal))]
public static class LogicRoleSelectionNormalPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LogicRoleSelectionNormal.AssignRolesForTeam))]
    public static bool AssignRolesForTeam(
        LogicRoleSelectionNormal __instance,
        List<NetworkedPlayerInfo> players,
        IGameOptions opts,
        RoleTeamTypes team,
        int teamMax,
        Il2CppSystem.Nullable<RoleTypes> defaultRole)
    {
        int num = 0;
        var source = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => role.TeamType == team && !RoleManager.IsGhostRole(role.Role) &&
                           CustomRoleUtils.CanSpawnOnCurrentMode(role));
        List<RoleTypes> list = new List<RoleTypes>();
        IRoleOptionsCollection roleOptions = opts.RoleOptions;

        var assignmentData = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();
        var source2 = CustomRoleUtils.GetPossibleRoles(assignmentData, x => x.Chance == 100);
        var guaranteedRoles = source.Where(x => source2.Contains(((ushort)x.Role, 100)));
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

        __instance.AssignRolesFromList(players, teamMax, list, ref num);

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

        __instance.AssignRolesFromList(players, teamMax, list, ref num);
        var defaultRole2 = team is RoleTeamTypes.Crewmate ? RoleTypes.Crewmate : RoleTypes.Impostor;
        try
        {
            defaultRole2 = defaultRole.Value;
        }
        catch
        {
            // Ignored
        }

        while (list.Count < players.Count && list.Count + num < teamMax)
        {
            list.Add(defaultRole2);
        }

        __instance.AssignRolesFromList(players, teamMax, list, ref num);

        return false;
    }
}

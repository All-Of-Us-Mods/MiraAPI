using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(RoleManager))]
public static class SelectRolesPatch
{
    [SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Internal behaviour that does not need property-level validation.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
    [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Read above.")]
    [SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Read above.")] // why so many warnings???
    public static bool ApiHandlesRoleSelect = true;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleManager.SelectRoles))]
    public static bool SelectRoles()
    {
        if (!ApiHandlesRoleSelect)
        {
            return true;
        }
        Il2CppSystem.Collections.Generic.List<ClientData> list = new();
        AmongUsClient.Instance.GetAllClients(list);
        List<NetworkedPlayerInfo> list2 = [.. list.ToArray()
            .Where(c => c.Character != null && c.Character.Data != null && !c.Character.Data.Disconnected &&
                        !c.Character.Data.IsDead).OrderBy(c => c.Id).Select(c => c.Character.Data)];

        foreach (NetworkedPlayerInfo networkedPlayerInfo in GameData.Instance.AllPlayers)
        {
            if (networkedPlayerInfo.Object != null && networkedPlayerInfo.Object.isDummy)
            {
                list2.Add(networkedPlayerInfo);
            }
        }
        IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        int adjustedNumImpostors = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(list2.Count);
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Impostor, adjustedNumImpostors, RoleTypes.Impostor);
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Crewmate, int.MaxValue, RoleTypes.Crewmate);
        return false;
    }

    public static void AssignRolesForTeam(
        List<NetworkedPlayerInfo> players,
        IGameOptions opts,
        RoleTeamTypes team,
        int teamMax,
        RoleTypes defaultRole)
    {
        int num = 0;
        var source = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => role.TeamType == team && !RoleManager.IsGhostRole(role.Role) &&
                           CustomRoleUtils.CanSpawnOnCurrentMode(role));
        var list = new List<RoleTypes>();
        IRoleOptionsCollection roleOptions = opts.RoleOptions;

        // Assign guaranteed roles first, just like the vanilla selector. This is
        // important because the list of players is shared by both team passes.
        foreach (var role in source.Where(x => roleOptions.GetChancePerGame(x.Role) == 100)
                     .Select(role => role.Role))
        {
            for (var i = 0; i < roleOptions.GetNumPerGame(role); i++)
            {
                list.Add(role);
            }
        }

        AssignRolesFromList(players, teamMax, list, ref num);

        // A 100% role was already assigned above. Including it here can consume
        // another player and, more importantly, leaves the fallback count wrong.
        list.Clear();
        foreach (var role in source.Where(x =>
                     roleOptions.GetChancePerGame(x.Role) is > 0 and < 100)
                     .Select(role => role.Role))
        {
            for (var i = 0; i < roleOptions.GetNumPerGame(role); i++)
            {
                if (HashRandom.Next(101) < roleOptions.GetChancePerGame(role))
                {
                    list.Add(role);
                }
            }
        }

        AssignRolesFromList(players, teamMax, list, ref num);

        // Assign the remaining players up to the requested team limit. The
        // vanilla list has been consumed by AssignRolesFromList at this point,
        // so checking list.Count can leave players unassigned.
        while (players.Count > 0 && num < teamMax)
        {
            list.Add(defaultRole);
            AssignRolesFromList(players, teamMax, list, ref num);
        }
    }
    private static void AssignRolesFromList(List<NetworkedPlayerInfo> players, int teamMax, List<RoleTypes> roleList, ref int rolesAssigned)
    {
        while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
        {
            int index = HashRandom.FastNext(roleList.Count);
            RoleTypes roleType = roleList[index];
            roleList.RemoveAt(index);
            int index2 = HashRandom.FastNext(players.Count);
            players[index2].Object.RpcSetRole(roleType, false);
            players.RemoveAt(index2);
            rolesAssigned++;
        }
    }
}

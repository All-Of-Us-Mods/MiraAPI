using System;
using AmongUs.GameOptions;
using BepInEx.Configuration;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Text;
using HarmonyLib;
using MiraAPI.Networking;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Roles;

public interface ICustomRole
{
    string RoleName { get; }

    string RoleDescription { get; }

    string RoleLongDescription { get; }

    Color RoleColor { get; }

    ModdedRoleTeams Team { get; }
    
    LoadableAsset<Sprite> OptionsScreenshot { get; }

    LoadableAsset<Sprite> Icon => MiraAssets.Empty;

    ConfigDefinition NumConfigDefinition => new("Roles", $"Num{RoleName}");

    ConfigDefinition ChanceConfigDefinition => new("Roles", $"Chance{RoleName}");

    bool AffectedByLight => Team == ModdedRoleTeams.Crewmate;

    bool CanGetKilled => Team == ModdedRoleTeams.Crewmate;

    bool IsNeutral => Team == ModdedRoleTeams.Neutral;

    bool CanKill => Team == ModdedRoleTeams.Impostor;

    bool CanUseVent => Team == ModdedRoleTeams.Impostor;

    bool TasksCount => Team == ModdedRoleTeams.Crewmate;

    bool IsGhostRole => false;

    bool TargetsBodies => false;

    bool CreateCustomTab => true;

    bool HideSettings => IsGhostRole;

    RoleTypes GhostRole => Team == ModdedRoleTeams.Crewmate ? RoleTypes.CrewmateGhost : RoleTypes.ImpostorGhost;

    void CreateOptions() { }

    void PlayerControlFixedUpdate(PlayerControl playerControl) { }

    void HudUpdate(HudManager hudManager) { }

    string GetCustomEjectionMessage(NetworkedPlayerInfo player)
    {
        return Team == ModdedRoleTeams.Impostor ? $"{player.PlayerName} was The {RoleName}" : null;
    }

    StringBuilder SetTabText()
    {
        var taskStringBuilder = Helpers.CreateForRole(this);
        return taskStringBuilder;
    }
    
    NetData GetNetData(RoleBehaviour roleBehaviour)
    {
        PluginSingleton<MiraApiPlugin>.Instance.Config.TryGetEntry<int>(NumConfigDefinition, out var numEntry);
        PluginSingleton<MiraApiPlugin>.Instance.Config.TryGetEntry<int>(ChanceConfigDefinition, out var chanceEntry);
            
        return new NetData((uint)roleBehaviour.Role, BitConverter.GetBytes(numEntry.Value).AddRangeToArray(BitConverter.GetBytes(chanceEntry.Value)));
    }
}
using AmongUs.GameOptions;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Teams.Teams;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System;
using System.Text;
using UnityEngine;

namespace MiraAPI.Roles;

public interface ICustomRole
{
    string RoleName { get; }

    string RoleDescription { get; }

    string RoleLongDescription { get; }

    Color RoleColor { get; }

    Type Team { get; }

    LoadableAsset<Sprite> OptionsScreenshot => MiraAssets.Empty;

    LoadableAsset<Sprite> Icon => MiraAssets.Empty;

    ConfigDefinition NumConfigDefinition => new("Roles", $"Num{RoleName}");

    ConfigDefinition ChanceConfigDefinition => new("Roles", $"Chance{RoleName}");

    bool AffectedByLight => Team == typeof(CrewmateTeam);

    bool CanGetKilled => Team == typeof(CrewmateTeam);

    bool CanKill => Team == typeof(ImpostorTeam);

    bool CanUseVent => Team == typeof(ImpostorTeam);

    bool TasksCount => Team == typeof(CrewmateTeam);

    bool IsGhostRole => false;

    bool TargetsBodies => false;

    bool CreateCustomTab => true;

    bool HideSettings => IsGhostRole;

    RoleTypes GhostRole => Team == typeof(CrewmateRole) ? RoleTypes.CrewmateGhost : RoleTypes.ImpostorGhost;

    MiraPluginInfo ParentMod => CustomRoleManager.FindParentMod(this);

    void PlayerControlFixedUpdate(PlayerControl playerControl) { }

    void HudUpdate(HudManager hudManager) { }

    string GetCustomEjectionMessage(NetworkedPlayerInfo player)
    {
        return Team == typeof(ImpostorTeam) ? $"{player.PlayerName} was The {RoleName}" : null;
    }

    StringBuilder SetTabText()
    {
        var taskStringBuilder = Helpers.CreateForRole(this);
        return taskStringBuilder;
    }

    NetData GetNetData(RoleBehaviour roleBehaviour)
    {
        ParentMod.PluginConfig.TryGetEntry<int>(NumConfigDefinition, out var numEntry);
        ParentMod.PluginConfig.TryGetEntry<int>(ChanceConfigDefinition, out var chanceEntry);

        return new NetData((uint)roleBehaviour.Role, BitConverter.GetBytes(numEntry.Value).AddRangeToArray(BitConverter.GetBytes(chanceEntry.Value)));
    }
}
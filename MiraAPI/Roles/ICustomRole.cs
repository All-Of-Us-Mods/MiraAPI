using AmongUs.GameOptions;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
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

    ModdedRoleTeams Team { get; }

    int MaxPlayers => 15;

    LoadableAsset<Sprite> OptionsScreenshot => MiraAssets.Empty;

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

    bool CreateCustomTab => true;

    bool HideSettings => IsGhostRole;

    RoleTypes GhostRole => Team == ModdedRoleTeams.Crewmate ? RoleTypes.CrewmateGhost : RoleTypes.ImpostorGhost;

    MiraPluginInfo ParentMod => CustomRoleManager.FindParentMod(this);

    /// <summary>
    /// Runs on the PlayerControl FixedUpdate method for any player with this role
    /// </summary>
    /// <param name="playerControl">The PlayerControl that has this role</param>
    void PlayerControlFixedUpdate(PlayerControl playerControl) { }

    /// <summary>
    /// Only runs for local player on HudManager Update method
    /// </summary>
    /// <param name="hudManager">Reference to HudManager instance</param>
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

    bool IsModifierApplicable(BaseModifier modifier)
    {
        return true;
    }

    NetData GetNetData()
    {
        ParentMod.PluginConfig.TryGetEntry<int>(NumConfigDefinition, out var numEntry);
        ParentMod.PluginConfig.TryGetEntry<int>(ChanceConfigDefinition, out var chanceEntry);

        return new NetData(RoleId.Get(GetType()), BitConverter.GetBytes(numEntry.Value).AddRangeToArray(BitConverter.GetBytes(chanceEntry.Value)));
    }
}
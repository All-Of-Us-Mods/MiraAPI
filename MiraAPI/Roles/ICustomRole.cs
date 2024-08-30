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

    ConfigDefinition NumConfigDefinition => new("Roles", $"Num {this.GetType().FullName}");

    ConfigDefinition ChanceConfigDefinition => new("Roles", $"Chance {this.GetType().FullName}");

    bool AffectedByLight => this.Team == ModdedRoleTeams.Crewmate;

    bool CanGetKilled => this.Team == ModdedRoleTeams.Crewmate;

    bool CanKill => this.Team == ModdedRoleTeams.Impostor;

    bool CanUseVent => this.Team == ModdedRoleTeams.Impostor;

    bool TasksCount => this.Team == ModdedRoleTeams.Crewmate;

    bool IsGhostRole => false;

    bool CreateCustomTab => true;

    bool HideSettings => this.IsGhostRole;

    RoleTypes GhostRole => this.Team == ModdedRoleTeams.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;

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

    string? GetCustomEjectionMessage(NetworkedPlayerInfo player)
    {
        return this.Team == ModdedRoleTeams.Impostor ? $"{player.PlayerName} was The {this.RoleName}" : null;
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
        this.ParentMod.PluginConfig.TryGetEntry<int>(this.NumConfigDefinition, out var numEntry);
        this.ParentMod.PluginConfig.TryGetEntry<int>(this.ChanceConfigDefinition, out var chanceEntry);

        return new NetData(RoleId.Get(this.GetType()), BitConverter.GetBytes(numEntry.Value).AddRangeToArray(BitConverter.GetBytes(chanceEntry.Value)));
    }
}

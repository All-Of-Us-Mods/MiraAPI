using AmongUs.GameOptions;
using BepInEx.Configuration;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Text;
using UnityEngine;

namespace MiraAPI.Roles;

public interface ICustomRole
{
    string RoleName { get; }

    string RoleDescription { get; }

    string RoleLongDescription { get; }

    Color RoleColor { get; }

    RoleTeamTypes Team { get; }

    LoadableAsset<Sprite> Icon => MiraAssets.Empty;

    ConfigDefinition NumConfigDefinition => new("Roles", $"Num{RoleName}");

    ConfigDefinition ChanceConfigDefinition => new("Roles", $"Chance{RoleName}");

    bool AffectedByLight => Team == RoleTeamTypes.Crewmate;

    bool CanGetKilled => Team == RoleTeamTypes.Crewmate;

    bool IsOutcast => false;

    bool CanKill => Team == RoleTeamTypes.Impostor;

    bool CanUseVent => Team == RoleTeamTypes.Impostor;

    bool TasksCount => Team == RoleTeamTypes.Crewmate;

    bool IsGhostRole => false;

    bool TargetsBodies => false;

    bool CreateCustomTab => true;

    bool HideSettings => IsGhostRole;

    RoleTypes GhostRole => Team == RoleTeamTypes.Crewmate ? RoleTypes.CrewmateGhost : RoleTypes.ImpostorGhost;

    void CreateOptions() { }

    void PlayerControlFixedUpdate(PlayerControl playerControl) { }

    void HudUpdate(HudManager hudManager) { }

    string GetCustomEjectionMessage(GameData.PlayerInfo player)
    {
        return Team == RoleTeamTypes.Impostor ? $"{player.PlayerName} was The {RoleName}" : null;
    }

    StringBuilder SetTabText()
    {
        var taskStringBuilder = Helpers.CreateForRole(this);
        return taskStringBuilder;
    }
}
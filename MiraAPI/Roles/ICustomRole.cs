using AmongUs.GameOptions;
using BepInEx.Configuration;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Text;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Interface for custom roles.
/// </summary>
public interface ICustomRole
{
    /// <summary>
    /// Gets the name of the role.
    /// </summary>
    string RoleName { get; }

    /// <summary>
    /// Gets the description of the role. Used in the Intro Cutscene.
    /// </summary>
    string RoleDescription { get; }

    /// <summary>
    /// Gets the long description of the role. Used in the Role Tab and Role Options.
    /// </summary>
    string RoleLongDescription { get; }

    /// <summary>
    /// Gets the color of the role.
    /// </summary>
    Color RoleColor { get; }

    /// <summary>
    /// Gets the team of the role.
    /// </summary>
    ModdedRoleTeams Team { get; }

    /// <summary>
    /// Gets the **maximum amount** of players that can have this role at a time. This is not the same as the **number** of players that can have this role in a game.
    /// </summary>
    int MaxPlayers => 15;

    /// <summary>
    /// Gets the Sprite used for the Role Options menu screenshot.
    /// </summary>
    LoadableAsset<Sprite> OptionsScreenshot => MiraAssets.Empty;

    /// <summary>
    /// Gets the Sprite used for the Role Icon.
    /// </summary>
    LoadableAsset<Sprite> Icon => MiraAssets.Empty;

    /// <summary>
    /// Gets the BepInEx ConfigDefinition for the amount of players that can have this role.
    /// </summary>
    ConfigDefinition NumConfigDefinition => new("Roles", $"Num {GetType().FullName}");

    /// <summary>
    /// Gets the BepInEx ConfigDefinition for the chance of this role being selected.
    /// </summary>
    ConfigDefinition ChanceConfigDefinition => new("Roles", $"Chance {GetType().FullName}");

    /// <summary>
    /// Gets a value indicating whether the role is affected by light sabotages.
    /// </summary>
    bool AffectedByLight => Team == ModdedRoleTeams.Crewmate;

    /// <summary>
    /// Gets a value indicating whether the role can be killed by others.
    /// </summary>
    bool CanGetKilled => Team == ModdedRoleTeams.Crewmate;

    /// <summary>
    /// Gets a value indicating whether the role can kill others.
    /// </summary>
    bool CanKill => Team == ModdedRoleTeams.Impostor;

    /// <summary>
    /// Gets a value indicating whether the role can use vents.
    /// </summary>
    bool CanUseVent => Team == ModdedRoleTeams.Impostor;

    /// <summary>
    /// Gets a value indicating whether the role's tasks count towards task progress.
    /// </summary>
    bool TasksCount => Team == ModdedRoleTeams.Crewmate;

    /// <summary>
    /// Gets a value indicating whether the role is a Ghost.
    /// </summary>
    bool IsGhostRole => false;

    /// <summary>
    /// Unused for some reason.
    /// </summary>
    // TODO: change this to an enum
    bool CreateCustomTab => true;

    /// <summary>
    /// Gets a value indicating whether the role should show up in the Role Options menu.
    /// </summary>
    bool HideSettings => IsGhostRole;

    /// <summary>
    /// Gets the Ghost role that is applied when the player is killed.
    /// </summary>
    RoleTypes GhostRole => Team == ModdedRoleTeams.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;

    /// <summary>
    /// Gets the parent mod of this role.
    /// </summary>
    MiraPluginInfo ParentMod => CustomRoleManager.FindParentMod(this);

    /// <summary>
    /// This method runs on the PlayerControl.FixedUpdate method for ALL players with this role.
    /// </summary>
    /// <param name="playerControl">The PlayerControl that has this role.</param>
    void PlayerControlFixedUpdate(PlayerControl playerControl)
    {
    }

    /// <summary>
    /// This method runs on the HudManager.Update method ONLY when the LOCAL player has this role.
    /// </summary>
    /// <param name="hudManager">Reference to HudManager instance.</param>
    void HudUpdate(HudManager hudManager)
    {
    }

    /// <summary>
    /// Gets a custom ejection message for the role. Return null to use the default message.
    /// </summary>
    /// <param name="player">The NetworkedPlayerInfo object for this player.</param>
    /// <returns>A string with a custom ejection message or null.</returns>
    string? GetCustomEjectionMessage(NetworkedPlayerInfo player)
    {
        return Team == ModdedRoleTeams.Impostor ? $"{player.PlayerName} was The {RoleName}" : null;
    }

    /// <summary>
    /// Get the custom Role Tab text for this role.
    /// </summary>
    /// <returns>A StringBuilder with the role tab text.</returns>
    StringBuilder SetTabText() => Helpers.CreateForRole(this);

    /// <summary>
    /// Determine whether a given modifier can be applied to this role.
    /// </summary>
    /// <param name="modifier">The modifier to be tested.</param>
    /// <returns>True if the modifier is valid on this role, false otherwise.</returns>
    bool IsModifierApplicable(BaseModifier modifier)
    {
        return true;
    }
}

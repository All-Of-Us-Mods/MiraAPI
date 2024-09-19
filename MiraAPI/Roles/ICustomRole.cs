using System.Text;
using BepInEx.Configuration;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using Reactor.Utilities;
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
    /// Gets the advanced settings of the role.
    /// </summary>
    CustomRoleConfiguration Configuration { get; }

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

    internal ConfigDefinition NumConfigDefinition => new("Roles", $"Num {GetType().FullName}");
    internal ConfigDefinition ChanceConfigDefinition => new("Roles", $"Chance {GetType().FullName}");

    /// <summary>
    /// Gets the role chance option.
    /// </summary>
    /// <returns>The role chance option.</returns>
    public int? GetChance()
    {
        if (!Configuration.CanModifyChance)
        {
            return Configuration.DefaultChance;
        }

        if (ParentMod.PluginConfig.TryGetEntry(ChanceConfigDefinition, out ConfigEntry<int> entry))
        {
            return Mathf.Clamp(entry.Value, 0, 100);
        }

        return null;
    }

    /// <summary>
    /// Gets the role count option.
    /// </summary>
    /// <returns>The role count option.</returns>
    public int? GetCount()
    {
        if (ParentMod.PluginConfig.TryGetEntry(NumConfigDefinition, out ConfigEntry<int> entry))
        {
            return Mathf.Clamp(entry.Value, 0, Configuration.MaxRoleCount);
        }

        return null;
    }

    /// <summary>
    /// Sets the role chance option.
    /// </summary>
    /// <param name="chance">The chance between 0 and 100.</param>
    public void SetChance(int chance)
    {
        if (!Configuration.CanModifyChance)
        {
            Logger<MiraApiPlugin>.Error($"Cannot modify chance for role: {RoleName}");
            return;
        }

        if (ParentMod.PluginConfig.TryGetEntry(ChanceConfigDefinition, out ConfigEntry<int> entry))
        {
            entry.Value = Mathf.Clamp(chance, 0, 100);
            return;
        }

        Logger<MiraApiPlugin>.Error($"Error getting chance configuration for role: {RoleName}");
    }

    /// <summary>
    /// Sets the role count option.
    /// </summary>
    /// <param name="count">The amount of this role between zero and its MaxRoleCount in the Configuration.</param>
    public void SetCount(int count)
    {
        if (ParentMod.PluginConfig.TryGetEntry(NumConfigDefinition, out ConfigEntry<int> entry))
        {
            entry.Value = Mathf.Clamp(count, 0, Configuration.MaxRoleCount);
            return;
        }

        Logger<MiraApiPlugin>.Error($"Error getting count configuration for role: {RoleName}");
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

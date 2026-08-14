using System;
using System.Text;
using BepInEx.Configuration;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Interface for custom roles.
/// </summary>
public interface ICustomRole : IOptionable
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
    /// Gets the <see cref="Color"/> of the role.
    /// </summary>
    Color RoleColor { get; }

    /// <summary>
    /// Gets the <see cref="Color"/> that should be used in the options menu.
    /// </summary>
    Color OptionsMenuColor => RoleColor;

    /// <summary>
    /// Gets the <see cref="ModdedRoleTeams"/> of the role.
    /// </summary>
    ModdedRoleTeams Team { get; }

    /// <summary>
    /// Gets advanced settings of the role.
    /// </summary>
    CustomRoleConfiguration Configuration { get; }

    /// <summary>
    /// Gets the <see cref="RoleOptionsGroup"/>.
    /// </summary>
    RoleOptionsGroup RoleOptionsGroup => Team switch
    {
        ModdedRoleTeams.Crewmate => RoleOptionsGroup.Crewmate,
        ModdedRoleTeams.Impostor => RoleOptionsGroup.Impostor,
        ModdedRoleTeams.Custom => RoleOptionsGroup.Neutral,
        _ => new RoleOptionsGroup(RoleName, RoleColor),
    };

    /// <summary>
    /// Gets the role's TeamIntroCutscene configuration.
    /// </summary>
    TeamIntroConfiguration? IntroConfiguration => Team switch
    {
        ModdedRoleTeams.Custom => TeamIntroConfiguration.Neutral,
        _ => null,
    };

    /// <summary>
    /// Gets the parent mod of this role.
    /// </summary>
    MiraPluginInfo ParentMod => CustomRoleManager.FindParentMod(this);

    internal ConfigDefinition NumConfigDefinition => new("Roles", $"Num {GetType().FullName}");
    internal ConfigDefinition ChanceConfigDefinition => new("Roles", $"Chance {GetType().FullName}");

    /// <summary>
    /// Binds the configuration options for this role to the provided ConfigFile.
    /// </summary>
    /// <param name="config">The ConfigFile to bind the options to.</param>
    virtual void BindConfig(ConfigFile config)
    {
        config.Bind(NumConfigDefinition, Configuration.DefaultRoleCount);
        config.Bind(ChanceConfigDefinition, Configuration.DefaultChance);
    }

    /// <summary>
    /// Saves the role's configuration to a preset <see cref="ConfigFile"/>.
    /// </summary>
    /// <param name="presetConfig">The <see cref="ConfigFile"/> to save the preset configuration to.</param>
    /// <param name="useDefault">Whether to use the default values for the configuration.</param>
    virtual void SaveToPreset(ConfigFile presetConfig, bool useDefault = false)
    {
        BindConfig(presetConfig);
        presetConfig[NumConfigDefinition].BoxedValue = useDefault ? Configuration.DefaultRoleCount : GetCount();
        presetConfig[ChanceConfigDefinition].BoxedValue = useDefault ? Configuration.DefaultChance : GetChance();
    }

    /// <summary>
    /// Loads the role's configuration from a preset <see cref="ConfigFile"/>.
    /// </summary>
    /// <param name="presetConfig">The <see cref="ConfigFile"/> containing the preset configuration.</param>
    virtual void LoadFromPreset(ConfigFile presetConfig)
    {
        if (presetConfig.TryGetEntry(NumConfigDefinition, out ConfigEntry<int> numEntry))
        {
            SetCount(numEntry.Value);
        }

        if (presetConfig.TryGetEntry(ChanceConfigDefinition, out ConfigEntry<int> chanceEntry))
        {
            SetChance(chanceEntry.Value);
        }
    }

    /// <summary>
    /// Gets the role chance option.
    /// </summary>
    /// <returns>The role chance option.</returns>
    virtual int? GetChance()
    {
        return !Configuration.CanModifyChance
            ? Configuration.DefaultChance
            : ParentMod.PluginConfig.TryGetEntry(ChanceConfigDefinition, out ConfigEntry<int> entry)
                ? Mathf.Clamp(entry.Value, 0, 100)
                : null;
    }

    /// <summary>
    /// Gets the role count option.
    /// </summary>
    /// <returns>The role count option.</returns>
    virtual int? GetCount()
    {
        return ParentMod.PluginConfig.TryGetEntry(NumConfigDefinition, out ConfigEntry<int> entry)
            ? Mathf.Clamp(entry.Value, 0, Configuration.MaxRoleCount)
            : null;
    }

    /// <summary>
    /// Sets the role chance option.
    /// </summary>
    /// <param name="chance">The chance between 0 and 100.</param>
    virtual void SetChance(int chance)
    {
        if (!Configuration.CanModifyChance)
        {
            Error($"Cannot modify chance for role: {RoleName}");
            return;
        }

        if (ParentMod.PluginConfig.TryGetEntry(ChanceConfigDefinition, out ConfigEntry<int> entry))
        {
            entry.Value = Mathf.Clamp(chance, 0, 100);
            return;
        }

        Error($"Error getting chance configuration for role: {RoleName}");
    }

    /// <summary>
    /// Sets the role count option.
    /// </summary>
    /// <param name="count">The amount of this role between zero and its MaxRoleCount in the Configuration.</param>
    virtual void SetCount(int count)
    {
        if (ParentMod.PluginConfig.TryGetEntry(NumConfigDefinition, out ConfigEntry<int> entry))
        {
            entry.Value = Mathf.Clamp(count, 0, Configuration.MaxRoleCount);
            return;
        }

        Error($"Error getting count configuration for role: {RoleName}");
    }

    /// <summary>
    /// Whether the <see cref="PlayerControl.LocalPlayer"/> can see this role.
    /// </summary>
    /// <param name="player">The player with the role.</param>
    /// <returns>Whether they can see the role (name color) or not.</returns>
    virtual bool CanLocalPlayerSeeRole(PlayerControl player)
    {
        return (PlayerControl.LocalPlayer.Data.Role.IsImpostor && player.Data.Role.IsImpostor) || PlayerControl.LocalPlayer.Data.IsDead;
    }

    /// <summary>
    /// Allows the role to specify who is shown on the intro team screen.
    /// </summary>
    /// <param name="instance">The intro cutscene instance.</param>
    /// <param name="yourTeam">The reference to the list of player in the team.</param>
    /// <returns><see langword="true"/> to use the original team intro code, <see langword="false"/> to skip.</returns>
    virtual bool SetupIntroTeam(IntroCutscene instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (Team == ModdedRoleTeams.Custom)
        {
            var team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

            team.Add(PlayerControl.LocalPlayer);

            yourTeam = team;
        }

        return true;
    }

    /// <summary>
    /// Gets a custom ejection message for the role. Return <see langword="null"/> to use the default message.
    /// </summary>
    /// <param name="player">The NetworkedPlayerInfo object for this player.</param>
    /// <returns>A string with a custom ejection message or <see langword="null"/>.</returns>
    string? GetCustomEjectionMessage(NetworkedPlayerInfo player)
    {
        return Team == ModdedRoleTeams.Impostor ? $"{player.PlayerName} was The {RoleName}" : null;
    }

    /// <summary>
    /// Get the custom Role Tab text for this role.
    /// </summary>
    /// <returns>A <see cref="StringBuilder"/> with the role tab text.</returns>
    StringBuilder SetTabText()
    {
        return CustomRoleUtils.CreateForRole(this);
    }

    /// <summary>
    /// Determine whether a given <see cref="BaseModifier"/> can be applied to this role.
    /// </summary>
    /// <param name="modifier">The modifier to be tested.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> is valid on this role, <see langword="false"/> otherwise.</returns>
    bool IsModifierApplicable(BaseModifier modifier)
    {
        return true;
    }

    /// <summary>
    /// Determines whether the role can spawn in general, accounting for gamemodes and everything else.
    /// </summary>
    /// <returns><see langword="true"/> if the role is able to spawn, otherwise <see langword="false"/>.</returns>
    virtual bool CanSpawnOnCurrentMode()
    {
        return !GameManager.Instance.IsHideAndSeek();
    }

    /// <summary>
    /// Gets the function that determines whether the role should be toggled on or off in the game settings.
    /// </summary>
    virtual Func<bool> VisibleInSettings => () => true;
}

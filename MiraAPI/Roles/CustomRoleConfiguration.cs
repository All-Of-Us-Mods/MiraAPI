using System;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameModes;
using MiraAPI.Patches.Freeplay;
using MiraAPI.Utilities.Assets;
using TMPro;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;

namespace MiraAPI.Roles;

/// <summary>
/// Used to configure the specific settings of a role.
/// </summary>
public record struct CustomRoleConfiguration
{
    [Obsolete("Default constructor is not supported. Please use the constructor that takes an ICustomRole parameter.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Will not be removed, as this throws instead.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public CustomRoleConfiguration()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        throw new NotImplementedException("Default constructor is not supported. Please use the constructor that takes an ICustomRole parameter.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRoleConfiguration"/> struct.
    /// </summary>
    /// <param name="role">The <see cref="ICustomRole"/> in which you are configuring.</param>
    public CustomRoleConfiguration(ICustomRole role)
    {
        var roleBehaviour = role as RoleBehaviour;

        AffectedByLightOnAirship = role.Team == ModdedRoleTeams.Crewmate;
        KillButtonOutlineColor = role.Team switch
        {
            ModdedRoleTeams.Impostor => Palette.ImpostorRed,
            ModdedRoleTeams.Crewmate => Palette.CrewmateBlue,
            _ => role.RoleColor,
        };
        GhostRole = role.Team is ModdedRoleTeams.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost;
        CanGetKilled = roleBehaviour?.IsDead == false && role.Team is not ModdedRoleTeams.Impostor;
        UseVanillaKillButton = role.Team is ModdedRoleTeams.Impostor;
        CanUseVent = role.Team is ModdedRoleTeams.Impostor;
        CanUseSabotage = role.Team is ModdedRoleTeams.Impostor;
        TasksCountForProgress = role.Team is ModdedRoleTeams.Crewmate;
        HideSettings = roleBehaviour?.IsDead == true;
        ShowInFreeplay = roleBehaviour?.IsDead == false;
        FreeplayFolder = role.Team switch
        {
            ModdedRoleTeams.Crewmate => TranslationController.Instance.GetString(StringNames.Crewmate),
            ModdedRoleTeams.Impostor => TranslationController.Instance.GetString(StringNames.Impostor),
            _ => TaskAdderPatches.NeutralName,
        };
        IntroSound = role.Team is ModdedRoleTeams.Crewmate
            ? CustomRoleManager.CrewmateIntroSound
            : CustomRoleManager.ImpostorIntroSound;
    }

    /// <summary>
    /// Gets or sets the hard limit of players that can have this role. This property is used to set a limit in the Role Options menu. If set to 0, the role will not be assigned at start.
    /// </summary>
    public int MaxRoleCount { get; set; } = 15;

    /// <summary>
    /// Gets or sets the default role count.
    /// </summary>
    public int DefaultRoleCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the default role chance.
    /// </summary>
    public int DefaultChance { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the chance option can be changed or not.
    /// </summary>
    public bool CanModifyChance { get; set; } = true;

    /// <summary>
    /// Gets or sets the <see cref="Sprite"/> used for the Role Options menu screenshot.
    /// </summary>
    [HideFromIl2Cpp]
    public LoadableAsset<Sprite>? OptionsScreenshot { get; set; } = null;

    /// <summary>
    /// Gets or sets the <see cref="Sprite"/> used for the Role Icon.
    /// </summary>
    [HideFromIl2Cpp]
    public LoadableAsset<Sprite>? Icon { get; set; } = null;

    /// <summary>
    /// Gets or sets the <see cref="TMP_SpriteAsset"/> for the Role Icon.
    /// </summary>
    [HideFromIl2Cpp]
    public TMP_SpriteAsset IconTmp { get; set; }

    /// <summary>
    /// Gets or sets the Intro sound for the Role.
    /// </summary>
    [HideFromIl2Cpp]
    public LoadableAsset<AudioClip>? IntroSound { get; set; } = null;

    /// <summary>
    /// Gets or sets the associated game mode for this role. This is used to determine if the role should be available in a specific game mode.
    /// </summary>
    public Type AssociatedGameMode { get; set; } = typeof(ClassicMode);

    /// <summary>
    /// Gets or sets a value indicating whether the role is affected by light affectors on Airship.
    /// </summary>
    public bool AffectedByLightOnAirship { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role can be killed by vanilla murder system.
    /// </summary>
    public bool CanGetKilled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role should use the vanilla kill button.
    /// </summary>
    public bool UseVanillaKillButton { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role can use <see cref="Vent"/>s. Also sends vanilla <see cref="Vent"/> data.
    /// </summary>
    public bool CanUseVent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role can receive vanilla <see cref="Vent"/> data.
    /// </summary>
    public bool GetsVentData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role can use the sabotage button.
    /// </summary>
    public bool CanUseSabotage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role's tasks count towards task progress.
    /// </summary>
    public bool TasksCountForProgress { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role should show up in the Role Options menu.
    /// </summary>
    public bool HideSettings { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role should show up in the Freeplay Role Selection menu.
    /// </summary>
    public bool ShowInFreeplay { get; set; }

    /// <summary>
    /// Gets or sets a freeplay folder this role belongs to. Defaults to team folder.
    /// </summary>
    public string FreeplayFolder { get; set; }

    /// <summary>
    /// Gets or sets the outline <see cref="Color"/> for the <see cref="KillButton"/> if <see cref="UseVanillaKillButton"/> is <see langword="true"/>.
    /// </summary>
    public Color KillButtonOutlineColor { get; set; }

    /// <summary>
    /// Gets or sets the role hint style. See <see cref="RoleHintType"/> <see langword="enum"/> for all options.
    /// </summary>
    public RoleHintType RoleHintType { get; set; } = RoleHintType.RoleTab;

    /// <summary>
    /// Gets or sets the Ghost role that is applied when the player is killed.
    /// </summary>
    public RoleTypes GhostRole { get; set; }
}

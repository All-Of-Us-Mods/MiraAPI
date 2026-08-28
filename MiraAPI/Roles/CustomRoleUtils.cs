using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameModes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Utilities to make handling roles in-game easier.
/// </summary>
public static class CustomRoleUtils
{
    /// <summary>
    /// Determines whether the specified <see cref="RoleBehaviour"/> can spawn in general, accounting for gamemodes and everything else.
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> you would like to check for.</param>
    /// <returns><see langword="true"/> if the <see cref="RoleBehaviour"/> is able to spawn, otherwise <see langword="false"/>.</returns>
    public static bool CanSpawnOnCurrentMode(RoleBehaviour role)
    {
        return role is ICustomRole custom
            ? custom.CanSpawnOnCurrentMode() && custom.Configuration.AssociatedGameMode.IsInstanceOfType(CustomGameModeManager.ActiveMode)
            : (CustomGameModeManager.ActiveMode is HideAndSeekMode
                ? role.Role is RoleTypes.Engineer or RoleTypes.Impostor
                : !Helpers.IsRoleBlacklisted(role));
    }

    /// <summary>
    /// Retrieves a flattened list of possible roles and their selection chances based on the provided assignment data.
    /// </summary>
    /// <param name="assignmentData">The list of role assignment configurations to process.</param>
    /// <param name="predicate">An optional filter to apply to the assignment data. If null, all roles in the list are processed.</param>
    /// <returns>A list of tuples containing the role type and its corresponding chance, duplicated according to each role's configured count.</returns>
    public static List<(ushort RoleType, int Chance)> GetPossibleRoles(
        List<RoleManager.RoleAssignmentData> assignmentData,
        Func<RoleManager.RoleAssignmentData, bool>? predicate = null)
    {
        var roles = new List<(ushort, int)>();

        assignmentData.Where(x => predicate == null || predicate(x)).ToList().ForEach(x =>
        {
            for (var i = 0; i < x.Count; i++)
            {
                roles.Add(((ushort)x.Role.Role, x.Chance));
            }
        });

        return roles;
    }

    /// <summary>
    /// Generates the role assignment data for a specific role type using the current game options.
    /// </summary>
    /// <param name="roleType">The type of the role to retrieve assignment data for.</param>
    /// <returns>A <see cref="RoleManager.RoleAssignmentData"/> instance containing the role behaviour, its maximum count per game, and its selection chance.</returns>
    public static RoleManager.RoleAssignmentData GetAssignData(RoleTypes roleType)
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var role = GetRegisteredRole(roleType);
        var assignmentData = new RoleManager.RoleAssignmentData(
            role,
            roleOptions.GetNumPerGame(role!.Role),
            roleOptions.GetChancePerGame(role.Role));

        return assignmentData;
    }

    /// <summary>
    /// Retrieves the registered behaviour for a given role type.
    /// </summary>
    /// <param name="roleType">The type of the role to find.</param>
    /// <returns>The matching <see cref="RoleBehaviour"/> if found; otherwise, <c>null</c>.</returns>
    public static RoleBehaviour? GetRegisteredRole(RoleTypes roleType)
    {
        // we want to prioritize the custom roles because the role has the right RoleColour/TeamColor
        return CustomRoleManager.AllRoles.FirstOrDefault(x => x.Role == roleType);
    }

    /// <summary>
    /// Gets all active in-game roles.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="RoleBehaviour"/>s.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<RoleBehaviour> GetActiveRoles()
    {
        return PlayerControl.AllPlayerControls.ToArray().Select(x => x.Data.Role);
    }

    /// <summary>
    /// Gets all active in-game roles in a certain team.
    /// </summary>
    /// <param name="team">The team you would like to check for.</param>
    /// <returns>A list of roles with the team.</returns>
    public static IEnumerable<RoleBehaviour> GetActiveRolesOfTeam(ModdedRoleTeams team)
    {
        return GetActiveRoles().Where(x => x is ICustomRole customRole && customRole.Team == team);
    }

    /// <summary>
    /// Gets all active in-game <typeparamref name="T"/> roles.
    /// </summary>
    /// <typeparam name="T">The <see cref="RoleBehaviour"/> you would like to check for.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s.</returns>
    public static IEnumerable<T> GetActiveRolesOfType<T>() where T : RoleBehaviour
    {
        return GetActiveRoles().OfType<T>();
    }

    /// <summary>
    /// Creates a <see cref="StringBuilder"/> for the Role Tab.
    /// </summary>
    /// <param name="role">The <see cref="ICustomRole"/> object.</param>
    /// <returns>A <see cref="StringBuilder"/>.</returns>
    public static StringBuilder CreateForRole(ICustomRole role)
    {
        var taskStringBuilder = new StringBuilder();
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleColor.ToTextColor()}Your role is <b>{role.RoleName}.</b></color>");
        taskStringBuilder.Append("<size=70%>");
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");
        return taskStringBuilder;
    }

    /// <summary>
    /// Returns an intro sound from a role.
    /// </summary>
    /// <param name="roleType">The role type.</param>
    /// <returns>The intro <see cref="AudioClip"/>.</returns>
    public static LoadableAsset<AudioClip>? GetIntroSound(RoleTypes roleType)
    {
        var role = CustomRoleManager.AllRoles.FirstOrDefault(role => role.Role == roleType);
        return role is ICustomRole customRole ? customRole.Configuration.IntroSound : new PreloadedAsset<AudioClip>(role!.IntroSound);
    }

    /// <summary>
    /// Determines if a <see cref="RoleBehaviour"/> is a custom role or not.
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> to check.</param>
    /// <returns><see langword="true"/> if the <see cref="RoleBehaviour"/> is a custom role, <see langword="false"/> otherwise.</returns>
    public static bool IsCustomRole(this RoleBehaviour role)
    {
        return CustomRoleManager.CustomRoles.ContainsKey((ushort)role.Role);
    }
}

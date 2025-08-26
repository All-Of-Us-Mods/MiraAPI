using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Utilities to make handling roles in-game easier.
/// </summary>
public static class CustomRoleUtils
{
    /// <summary>
    /// Stores the mapping of each role type to its associated list of button types.
    /// </summary>
    private static readonly Dictionary<Type, List<Type>> _roleToButtonsMap = new();

    /// <summary>
    /// Gets a read-only view of the role-to-buttons mapping.
    /// </summary>
    public static IReadOnlyDictionary<Type, List<Type>> RoleToButtonsMap => _roleToButtonsMap;

    /// <summary>
    /// Gets all active in-game roles.
    /// </summary>
    /// <returns>A list of roles.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<RoleBehaviour> GetActiveRoles() => PlayerControl.AllPlayerControls.ToArray().Select(x => x.Data.Role);

    /// <summary>
    /// Gets all active in-game roles in a certain team.
    /// </summary>
    /// <param name="team">The team you would like to check for.</param>
    /// <returns>A list of roles with the team.</returns>
    public static IEnumerable<RoleBehaviour> GetActiveRolesOfTeam(ModdedRoleTeams team) => GetActiveRoles().Where(x => x is ICustomRole customRole && customRole.Team == team);

    /// <summary>
    /// Gets all active in-game roles of a certain type.
    /// </summary>
    /// <typeparam name="T">The role Type you would like to check for. Must be a RoleBehaviour.</typeparam>
    /// <returns>A list of roles with that specific type.</returns>
    public static IEnumerable<T> GetActiveRolesOfType<T>() where T : RoleBehaviour => GetActiveRoles().OfType<T>();

    /// <summary>
    /// Creates a string builder for the Role Tab.
    /// </summary>
    /// <param name="role">The ICustomRole object.</param>
    /// <returns>A StringBuilder.</returns>
    public static StringBuilder CreateForRole(ICustomRole role)
    {
        var taskStringBuilder = new StringBuilder();
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleColor.ToTextColor()}Your role is <b>{role.RoleName}.</b></color>");
        taskStringBuilder.Append("<size=70%>");
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");
        return taskStringBuilder;
    }

    /// <summary>
    /// Builds the RoleToButtonsMap by evaluating every role against all registered buttons
    /// using the <see cref="CustomActionButton.Enabled(RoleBehaviour)"/> method.
    /// </summary>
    public static void BuildRoleButtonMap()
    {
        _roleToButtonsMap.Clear();

        foreach (var role in CustomRoleManager.CustomRoles.Values)
        {
            var roleType = role.GetType();
            var buttonsForRole = new List<Type>();

            foreach (var btn in CustomButtonManager.Buttons)
            {
                if (btn.Enabled(role))
                {
                    var bt = btn.GetType();
                    if (!buttonsForRole.Contains(bt))
                        buttonsForRole.Add(bt);
                }
            }
            if (buttonsForRole.Count > 0)
            {
                _roleToButtonsMap[roleType] = buttonsForRole;

                Logger<MiraApiPlugin>.Instance.LogInfo(
                   $"Mapped {roleType.Name} -> [{string.Join(", ", buttonsForRole.Select(x => x.Name))}]"
               );
            }
        }
    }

    /// <summary>
    /// Gets the list of button types associated with the specified role instance.
    /// </summary>
    /// <param name="role">The custom role instance for which to retrieve associated button types.</param>
    /// <returns>
    /// A read-only list of button <see cref="Type"/> objects associated with the given role.
    /// </returns>
    public static IReadOnlyList<Type> GetButtonsForRole(ICustomRole role)
    {
        var roleType = role.GetType();
        _roleToButtonsMap.TryGetValue(roleType, out var list);
        return list;
    }

    /// <summary>
    /// Returns an intro sound from a role.
    /// </summary>
    /// <param name="roleType">The role type.</param>
    /// <returns>The intro sound.</returns>
    public static LoadableAsset<AudioClip>? GetIntroSound(RoleTypes roleType)
    {
        var role = RoleManager.Instance.AllRoles.FirstOrDefault(role => role.Role == roleType);
        if (role is ICustomRole customRole)
        {
            return customRole.Configuration.IntroSound;
        }

        return new PreloadedAsset<AudioClip>(role!.IntroSound);
    }

    /// <summary>
    /// Determines if a role is a custom role or not.
    /// </summary>
    /// <param name="role">The RoleBehaviour to check.</param>
    /// <returns>True if the role is a custom role, false otherwise.</returns>
    public static bool IsCustomRole(this RoleBehaviour role)
    {
        return CustomRoleManager.CustomRoles.ContainsKey((ushort)role.Role);
    }
}

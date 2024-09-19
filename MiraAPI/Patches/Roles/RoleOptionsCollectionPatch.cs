using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using Reactor.Utilities;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Patches to return the correct role counts.
/// </summary>
[HarmonyPatch(typeof(RoleOptionsCollectionV08))]
public static class RoleOptionsCollectionPatch
{
    /// <summary>
    /// Set the role chance for custom Launchpad roles based on config
    /// </summary>
    /// <returns>Return false to skip original method, true to not.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleOptionsCollectionV08.GetChancePerGame))]
    public static bool GetChancePrefix(RoleTypes role, ref int __result)
    {
        if (!CustomRoleManager.GetCustomRoleBehaviour(role, out var customRole) || customRole == null)
        {
            return true;
        }

        if (customRole.Configuration.HideSettings)
        {
            __result = 0;
            return false;
        }

        var chance = customRole.GetChance();
        if (chance == null)
        {
            Logger<MiraApiPlugin>.Error($"Chance is null, defaulting to zero.");
            chance = 0;
        }

        __result = chance.Value;
        return false;
    }

    /// <summary>
    /// Set the amount for custom Launchpad roles based on config
    /// </summary>
    /// <returns>Return false to skip original method, true to not.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleOptionsCollectionV08.GetNumPerGame))]
    public static bool GetNumPrefix(RoleTypes role, ref int __result)
    {
        if (!CustomRoleManager.GetCustomRoleBehaviour(role, out var customRole) || customRole == null)
        {
            return true;
        }

        if (customRole.Configuration.HideSettings)
        {
            __result = 0;
            return false;
        }

        var count = customRole.GetCount();
        if (count == null)
        {
            Logger<MiraApiPlugin>.Error($"Count is null, defaulting to zero.");
            count = 0;
        }

        __result = count.Value;
        return false;
    }
}

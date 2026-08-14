using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Patches to return the correct role counts.
/// </summary>
[HarmonyPatch(typeof(RoleOptionsCollectionV10))]
public static class RoleOptionsCollectionPatch
{
    /// <summary>
    /// This patch fixes <see cref="RoleOptionsCollectionV10.GetNumPerGame(RoleTypes)"/> being inlined (2025.9.9) in the original code.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleOptionsCollectionV10.AnyRolesEnabled))]
    public static bool AnyRolesEnabledPrefix(RoleOptionsCollectionV10 __instance)
    {
        foreach (KeyValuePair<RoleTypes, RoleDataV10> keyValuePair in __instance.roles)
        {
            if (__instance.GetNumPerGame(keyValuePair.Key) > 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Set the role chance for custom Launchpad roles based on config.
    /// </summary>
    /// <returns>Return <see langword="false"/> to skip original method, <see langword="true"/> to not.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleOptionsCollectionV10.GetChancePerGame))]
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
            Error($"Chance is null, defaulting to zero.");
            chance = 0;
        }

        __result = chance.Value;
        return false;
    }

    /// <summary>
    /// Set the amount for custom Launchpad roles based on config.
    /// </summary>
    /// <returns>Return <see langword="false"/> to skip original method, <see langword="true"/> to not.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleOptionsCollectionV10.GetNumPerGame))]
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
            Error($"Count is null, defaulting to zero.");
            count = 0;
        }

        __result = count.Value;
        return false;
    }
}

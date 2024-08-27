using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(RoleOptionsCollectionV08))]
public static class RoleOptionsCollectionPatch
{
    /// <summary>
    /// Set the role chance for custom Launchpad roles based on config
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleOptionsCollectionV08.GetChancePerGame))]
    public static bool GetChancePrefix([HarmonyArgument(0)] RoleTypes roleType, ref int __result)
    {
        if (CustomRoleManager.GetCustomRoleBehaviour(roleType, out var customRole))
        {
            if (customRole.HideSettings)
            {
                return false;
            }

            customRole.ParentMod.PluginConfig.TryGetEntry<int>(customRole.ChanceConfigDefinition, out var entry);
            __result = entry.Value;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Set the amount for custom Launchpad roles based on config
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RoleOptionsCollectionV08.GetNumPerGame))]
    public static bool GetNumPrefix([HarmonyArgument(0)] RoleTypes roleType, ref int __result)
    {
        if (CustomRoleManager.GetCustomRoleBehaviour(roleType, out var customRole))
        {
            if (customRole.HideSettings)
            {
                return false;
            }

            customRole.ParentMod.PluginConfig.TryGetEntry<int>(customRole.NumConfigDefinition, out var entry);
            __result = entry.Value;
            return false;
        }

        return true;
    }
}

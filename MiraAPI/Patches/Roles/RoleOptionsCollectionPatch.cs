using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using Reactor.Utilities;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(RoleOptionsCollectionV07))]
public static class RoleOptionsCollectionPatch
{
    /// <summary>
    /// Set the role chance for custom Launchpad roles based on config
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("GetChancePerGame")]
    public static bool GetChancePrefix([HarmonyArgument(0)] RoleTypes roleType, ref int __result)
    {
        if (CustomRoleManager.GetCustomRoleBehaviour(roleType, out var customRole))
        {
            if (customRole.HideSettings)
            {
                return false;
            }

            PluginSingleton<MiraAPIPlugin>.Instance.Config.TryGetEntry<int>(customRole.ChanceConfigDefinition, out var entry);
            __result = entry.Value;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Set the amount for custom Launchpad roles based on config
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("GetNumPerGame")]
    public static bool GetNumPrefix([HarmonyArgument(0)] RoleTypes roleType, ref int __result)
    {
        if (CustomRoleManager.GetCustomRoleBehaviour(roleType, out var customRole))
        {
            if (customRole.HideSettings)
            {
                return false;
            }

            PluginSingleton<MiraAPIPlugin>.Instance.Config.TryGetEntry<int>(customRole.NumConfigDefinition, out var entry);
            __result = entry.Value;
            return false;
        }

        return true;
    }
}

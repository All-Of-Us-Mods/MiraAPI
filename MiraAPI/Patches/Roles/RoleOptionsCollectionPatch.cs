using System;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Patches to return the correct role counts.
/// </summary>
public static class RoleOptionsCollectionPatch
{
    internal static Assembly BaseAssembly { get; private set; }
    internal static Type[] BaseTypes { get; private set; }
    internal static Type CollectionsType { get; private set; }
    public static void PatchRoleMethods(Harmony harmony)
    {
        var assembly = Array.Find(
            AppDomain.CurrentDomain.GetAssemblies(),
            ass => ass.GetName().Name == "Assembly-CSharp"
        );
        if (assembly != null)
        {
            var compatType = typeof(RoleOptionsCollectionPatch);
            BaseAssembly = assembly;
            BaseTypes = AccessTools.GetTypesFromAssembly(assembly);
            var collections = BaseTypes.Where(x => x.Name.Contains("RoleOptionsCollectionV")).ToArray();
            var pairings = new System.Collections.Generic.Dictionary<int, Type>();
            var newestId = 0;
            foreach (var collection in collections)
            {
                var remainer = collection.Name.Replace("RoleOptionsCollectionV", string.Empty);
                if (int.TryParse(remainer, out var id))
                {
                    pairings.Add(id, collection);
                    if (newestId < id)
                    {
                        newestId = id;
                    }
                }
            }

            if (pairings.TryGetValue(newestId, out var typeToGet))
            {
                CollectionsType = typeToGet;
            }

            var anyRolesEnabledMethod = AccessTools.Method(CollectionsType, "AnyRolesEnabled");
            harmony.Patch(
                anyRolesEnabledMethod,
                new HarmonyMethod(AccessTools.Method(compatType, nameof(AnyRolesEnabledPrefix))));

            var chancePerGameMethod = AccessTools.Method(CollectionsType, "GetChancePerGame");
            harmony.Patch(
                chancePerGameMethod,
                new HarmonyMethod(AccessTools.Method(compatType, nameof(GetChancePrefix))));

            var numPerGameMethod = AccessTools.Method(CollectionsType, "GetNumPerGame");
            harmony.Patch(
                numPerGameMethod,
                new HarmonyMethod(AccessTools.Method(compatType, nameof(GetNumPrefix))));
            Info($"Patched methods for RoleOptionsCollectionV{newestId}");
        }
    }
    /// <summary>
    /// This patch fixes <see cref="RoleOptionsCollectionV11.GetNumPerGame(RoleTypes)"/> being inlined (2025.9.9) in the original code.
    /// </summary>
    public static bool AnyRolesEnabledPrefix(dynamic __instance, ref bool __result)
    {
        foreach (KeyValuePair<RoleTypes, dynamic> keyValuePair in __instance.roles)
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

using System.Globalization;
using HarmonyLib;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Options;

/// <summary>
/// Patches for the various game settings.
/// </summary>
[HarmonyPatch]
public static class SettingPatches
{
    /// <summary>
    /// Prefix for the <see cref="FloatGameSetting.GetValueString"/> method. Adds support for custom number suffixes.
    /// </summary>
    /// <param name="__instance">The FloatGameSetting instance.</param>
    /// <param name="__result">The result of the GetValueString method.</param>
    /// <param name="value">The float value.</param>
    /// <returns>False to skip original method.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FloatGameSetting), nameof(FloatGameSetting.GetValueString))]
    public static bool ValueStringPatch(
        FloatGameSetting __instance,
        ref string __result,
        [HarmonyArgument(0)] float value)
    {
        string result;
        var suffix = (MiraNumberSuffixes)__instance.SuffixType;

        if (__instance.ZeroIsInfinity && Mathf.Abs(value) < 0.0001f)
        {
            result = "<b>∞</b>";
        }
        else
        {
            result = suffix switch
            {
                MiraNumberSuffixes.None => value.ToString(__instance.FormatString, NumberFormatInfo.InvariantInfo),
                MiraNumberSuffixes.Multiplier => value.ToString(__instance.FormatString, NumberFormatInfo.InvariantInfo) + "x",
                MiraNumberSuffixes.Percent => value.ToString(__instance.FormatString, NumberFormatInfo.InvariantInfo) + "%",
                _ => TranslationController.Instance.GetString(
                    StringNames.GameSecondsAbbrev,
                    (Il2CppSystem.Object[])[value.ToString(__instance.FormatString, CultureInfo.InvariantCulture)]),
            };
        }

        __result = result;
        return false;
    }
}

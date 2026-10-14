using System.Globalization;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
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
    /// <param name="__instance">The <see cref="FloatGameSetting"/> instance.</param>
    /// <param name="__result">The result of the <see cref="FloatGameSetting.GetValueString"/> method.</param>
    /// <param name="value">The <see langword="float"/> value.</param>
    /// <returns><see langword="false"/> to skip original method.</returns>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FloatGameSetting), nameof(FloatGameSetting.GetValueString))]
    public static bool ValueStringPatch(
        FloatGameSetting __instance,
        ref string __result,
        [HarmonyArgument(0)] float value)
    {
        string result;
        var suffix = (MiraNumberSuffixes)__instance.SuffixType;

        var custom =
            ModdedOptionsManager.ModdedOptions.Values.FirstOrDefault(opt =>
                opt.OptionBehaviour != null && opt.OptionBehaviour.Data == __instance);
        if (custom is ModdedNumberOption moddedNumberOption)
        {
            if (moddedNumberOption.NegativeWordValue != "#" && (int)value == -1)
            {
                result = $"<b>{moddedNumberOption.NegativeWordValue}</b>";
            }
            else if (moddedNumberOption.ZeroWordValue != "#" && Mathf.Abs(value) < 0.0001f)
            {
                result = $"<b>{moddedNumberOption.ZeroWordValue}</b>";
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
        }
        else if (__instance.ZeroIsInfinity && Mathf.Abs(value) < 0.0001f)
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

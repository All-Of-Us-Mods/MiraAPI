using HarmonyLib;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Options;
[HarmonyPatch]
public class SettingPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FloatGameSetting), nameof(FloatGameSetting.GetValueString))]
    public static bool ValueStringPatch(FloatGameSetting __instance, ref string __result, [HarmonyArgument(0)] float value)
    {
        var result = string.Empty;
        var suffix = (MiraNumberSuffixes)__instance.SuffixType;

        if (__instance.ZeroIsInfinity && Mathf.Abs(value) < 0.0001f)
        {
            result = "<b>∞</b>";
        }
        else if (suffix == MiraNumberSuffixes.None)
        {
            result = value.ToString(__instance.FormatString);
        }
        else if (suffix == MiraNumberSuffixes.Multiplier)
        {
            result = value.ToString(__instance.FormatString) + "x";
        }
        else if (suffix == MiraNumberSuffixes.Percent)
        {
            result = value.ToString(__instance.FormatString) + "%";
        }
        else
        {
            result = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, (Il2CppSystem.Object[])(
            [
                value.ToString(__instance.FormatString)
            ]));
        }
        __result = result;
        return false;
    }
}
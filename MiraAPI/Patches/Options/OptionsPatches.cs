using HarmonyLib;
using Il2CppSystem;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Options
{
    [HarmonyPatch]
    public static class OptionsPatches
    {
        [HarmonyPrefix, HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Initialize))]
        public static bool ToggleInit(ToggleOption __instance)
        {
            if (!__instance.IsCustom()) return true;
            __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.Title, Array.Empty<Object>());

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
        public static bool ToggleUpdate(ToggleOption __instance)
        {
            return !__instance.IsCustom();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
        public static bool NumberInit(NumberOption __instance)
        {
            if (!__instance.IsCustom()) return true;
            __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.Title, Array.Empty<Object>());

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
        public static bool NumberUpdate(NumberOption __instance)
        {
            return !__instance.IsCustom();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
        public static bool StringInit(StringOption __instance)
        {
            if (!__instance.IsCustom()) return true;
            __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.Title, Array.Empty<Object>());
            __instance.ValueText.text = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.Values[__instance.Value], Array.Empty<Object>());

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StringOption), nameof(StringOption.UpdateValue))]
        public static bool StringUpdate(StringOption __instance)
        {
            return !__instance.IsCustom();
        }
    }
}
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using MiraAPI.LocalSettings;
using UnityEngine;

namespace MiraAPI.Patches.Accessibility;

[HarmonyPatch]
[SuppressMessage("Style", "IDE0074:Use compound assignment", Justification = "Using compound assignment bypasses Unity lifetime checks.")]
public static class HudManagerFlashPatches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.StartReactorFlash))]
    [HarmonyPrefix]
    public static bool ReactorFlashPrefix(HudManager __instance)
    {
        if (__instance.ReactorFlash == null)
        {
            __instance.ReactorFlash = __instance.StartCoroutine(CoReactorFlash().WrapToIl2Cpp());
        }

        return false;
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.StartOxyFlash))]
    [HarmonyPrefix]
    public static bool OxygenFlashPrefix(HudManager __instance)
    {
        if (__instance.OxyFlash == null)
        {
            __instance.OxyFlash = __instance.StartCoroutine(CoReactorFlash().WrapToIl2Cpp());
        }

        return false;
    }
    public static IEnumerator CoReactorFlash()
    {
        if (!HudManager.InstanceExists)
        {
            yield break;
        }
        var hudManager = HudManager.Instance;
        var wait = new WaitForSeconds(1f);
        var light = false;

        hudManager.FullScreen.color = new Color(1f, 0f, 0f, 0.37254903f);
        while (true)
        {
            var settings = LocalSettingsTabSingleton<MiraApiSettings>.Instance;
            hudManager.FullScreen.gameObject.SetActive(settings.EnableSabotageFlashes.Value && !hudManager.FullScreen.gameObject.activeSelf);
            if (settings.EnableSabotageBlares.Value)
            {
                SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false);
            }
            light = !light;

            /*if (!MiraApiPlugin.IsMobile)
            {
                if (hudManager.lightFlashHandle == null)
                {
                    hudManager.lightFlashHandle = DualshockLightManager.Instance.AllocateLight();
                    hudManager.lightFlashHandle.color = new Color(1f, 0f, 0f, 1f);
                    hudManager.lightFlashHandle.intensity = 1f;
                }
                hudManager.lightFlashHandle.color.SetAlpha(light ? 1f : 0f);
            }*/
            yield return wait;
        }
    }
}

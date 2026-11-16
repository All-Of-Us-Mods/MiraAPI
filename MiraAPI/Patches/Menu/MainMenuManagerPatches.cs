using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using MiraAPI.LocalSettings;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Menu;

/// <summary>
/// General <see cref="MainMenuManager"/> patches.
/// </summary>
[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatches
{
    [SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Internal behaviour that does not need property-level validation.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
    internal static bool NeedsDeepDestroy;

    /// <summary>
    /// A postfix on <see cref="MainMenuManager.Awake"/> to load all the addressables registered.
    /// </summary>
    [HarmonyPatch(nameof(MainMenuManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix()
    {
        var requiredVersion = new Version(2026, 9, 29);
        var version = Version.Parse(Application.version);
        NeedsDeepDestroy = version >= requiredVersion;
        Coroutines.Start(SetFps());
    }

    private static IEnumerator SetFps()
    {
        Application.targetFrameRate = (int)LocalSettingsTabSingleton<MiraApiSettings>.Instance.SetFpsSlider.Value;
        yield return new WaitForSeconds(1f);

        Application.targetFrameRate = (int)LocalSettingsTabSingleton<MiraApiSettings>.Instance.SetFpsSlider.Value;
    }
}

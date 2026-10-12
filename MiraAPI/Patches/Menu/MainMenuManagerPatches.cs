using System;
using System.Collections;
using HarmonyLib;
using MiraAPI.LocalSettings;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Menu;

/// <summary>
/// General <see cref="MainMenuManager"/> patches.
/// </summary>
[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatches
{
    internal static bool NeedsDeepDestroy { get; private set; }

    /// <summary>
    /// A postfix on <see cref="MainMenuManager.Awake"/> to load all the addressables registered.
    /// </summary>
    [HarmonyPatch(nameof(MainMenuManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix()
    {
        var requiredVersion = new Version(2026, 6, 5);
        var version = Version.Parse(Application.version);
        NeedsDeepDestroy = version >= requiredVersion;
        AddressablesLoader.LoadAll();
        Coroutines.Start(SetFps());
    }

    private static IEnumerator SetFps()
    {
        Application.targetFrameRate = (int)LocalSettingsTabSingleton<MiraApiSettings>.Instance.SetFpsSlider.Value;
        yield return new WaitForSeconds(1f);

        Application.targetFrameRate = (int)LocalSettingsTabSingleton<MiraApiSettings>.Instance.SetFpsSlider.Value;
    }
}

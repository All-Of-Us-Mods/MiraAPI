using HarmonyLib;
using MiraAPI.Keybinds;
using Rewired;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(InputManager_Base), nameof(InputManager_Base.Awake))]
public static class KeybindMenuPatch
{
    private static bool _registered;

    [HarmonyPrefix]

    // ReSharper disable once InconsistentNaming
    private static void AwakePrefix(InputManager_Base __instance)
    {
        if (_registered) return;

        KeybindUtils.RewiredInputManager = __instance;
        KeybindManager.RewiredInit();
        _registered = true;

        // Set these here right after input manager initializes
        KeybindManager.SetKeybind<KillButton>(8);
        KeybindManager.SetKeybind<UseButton>(6);
        KeybindManager.SetKeybind<ReportButton>(7);
        KeybindManager.SetKeybind<VentButton>(50);
        KeybindManager.SetKeybind<SabotageButton>(4);
        KeybindManager.SetKeybind<AbilityButton>(49);
    }
}

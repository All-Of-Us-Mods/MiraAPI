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
        KeybindManager.VanillaKeybinds[typeof(KillButton)] = new VanillaKeybind(8);
        KeybindManager.VanillaKeybinds[typeof(UseButton)] = new VanillaKeybind(6);
        KeybindManager.VanillaKeybinds[typeof(ReportButton)] = new VanillaKeybind(7);
        KeybindManager.VanillaKeybinds[typeof(VentButton)] = new VanillaKeybind(50);
        KeybindManager.VanillaKeybinds[typeof(SabotageButton)] = new VanillaKeybind(4);
        KeybindManager.VanillaKeybinds[typeof(AbilityButton)] = new VanillaKeybind(49);
    }
}

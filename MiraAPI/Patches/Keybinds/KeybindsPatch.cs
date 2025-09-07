using HarmonyLib;
using MiraAPI.Keybinds;
using Rewired;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(InputManager_Base), nameof(InputManager_Base.Awake))]
public static class KeybindMenuPatch
{
    private static bool _registered;

    [HarmonyPrefix]
    private static void AwakePrefix(InputManager_Base __instance)
    {
        if (_registered)
        {
            return;
        }

        KeybindUtils.RewiredInputManager = __instance;
        KeybindManager.RewiredInit();
        _registered = true;
    }
}

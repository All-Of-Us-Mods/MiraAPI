using System.Linq;
using HarmonyLib;
using MiraAPI.Keybinds;
using Rewired;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(Player))]
public static class PlayerKeybindPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.GetButtonDown), typeof(int))]
    private static void GetButtonPostfix(Player __instance, int __0, ref bool __result)
    {
        if (!__result || __instance.id != 0 || PlayerControl.LocalPlayer == null || ConsoleJoystick.inputState != ConsoleJoystick.ConsoleInputState.Gameplay || ControllerManager.Instance.IsUiControllerActive) return;

        var keybind = KeybindManager.Keybinds.FirstOrDefault(x => x.RewiredInputAction?.id == __0);
        if (keybind is { Exclusive: false }) return;

        var sources = new CppCollections.List<InputActionSourceData>(__instance.GetCurrentInputSources(__0).Cast<CppCollections.IEnumerable<InputActionSourceData>>()).ToArray();
        if (sources.Length == 0 || sources.Any(x => x.controllerType != ControllerType.Joystick)) return;

        foreach (var source in sources)
        {
            var claimed = false;
            foreach (var custom in KeybindManager.Keybinds)
            {
                if (custom == keybind) break;
                if (!custom.Exclusive || custom.RewiredInputAction == null || !KeybindUtils.IsControllerActionActive(custom.RewiredInputAction.id)) continue;

                var inputs = new CppCollections.List<InputActionSourceData>(__instance.GetCurrentInputSources(custom.RewiredInputAction.id).Cast<CppCollections.IEnumerable<InputActionSourceData>>()).ToArray();
                if (!inputs.Any(x => x.controllerType == ControllerType.Joystick && x.controller.id == source.controller.id && x.actionElementMap.CheckForAssignmentConflict(source.actionElementMap))) continue;

                claimed = true;
                break;
            }

            if (!claimed) return;
        }

        __result = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.GetButton), typeof(int))]
    private static void GetButtonHeldPostfix(Player __instance, int __0, ref bool __result)
    {
        GetButtonPostfix(__instance, __0, ref __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.GetButtonDown), typeof(string))]
    private static void GetButtonByNamePostfix(Player __instance, string __0, ref bool __result)
    {
        if (__result) GetButtonPostfix(__instance, ReInput.mapping.GetActionId(__0), ref __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.GetButton), typeof(string))]
    private static void GetButtonHeldByNamePostfix(Player __instance, string __0, ref bool __result)
    {
        if (__result) GetButtonPostfix(__instance, ReInput.mapping.GetActionId(__0), ref __result);
    }
}

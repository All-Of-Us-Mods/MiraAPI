using System.Linq;
using HarmonyLib;
using MiraAPI.Keybinds;
using Rewired;
using Rewired.UI.ControlMapper;
using UnityEngine;
using UnityEngine.UI;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(ControlMapper))]
public static class ControlMapperPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ControlMapper.Initialize))]
    private static void InitializePrefix(ControlMapper __instance)
    {
        __instance.showControllers = true;
        __instance.showGlyphs = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ControlMapper.CreateLayout))]
    private static void CreateLayoutPostfix(ControlMapper __instance)
    {
        __instance.references.controllerGroup.gameObject.SetActive(false);
        __instance.references.assignedControllersGroup.gameObject.SetActive(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ControlMapper.GetControllerMap))]
    private static bool GetControllerMapPrefix(ControlMapper __instance, ControllerType type, ref ControllerMap? __result)
    {
        if (type != ControllerType.Joystick) return true;

        __result = __instance.currentPlayer?.controllers.maps.GetFirstMapInCategory(type, __instance.currentJoystickId, 1);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ControlMapper.HasElementAssignmentConflicts))]
    private static bool HasElementAssignmentConflictsPrefix(ControlMapper.InputMapping mapping, ref bool __result)
    {
        if (mapping == null || mapping.controllerType != ControllerType.Joystick || !KeybindManager.Keybinds.Any(x => x.RewiredInputAction?.id == mapping.fieldInfo.actionId)) return true;

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ControlMapper.Update))]
    private static void UpdatePrefix(ControlMapper __instance)
    {
        var conflicts = KeybindManager.GetConflicts();
        var controllerConflicts = KeybindManager.GetControllerConflicts();
        foreach (var element in __instance.themedElements)
        {
            var info = element.GetComponent<InputFieldInfo>();
            if (info == null) continue;

            var map = ReInput.mapping.GetActionElementMap(info.actionElementMapId);
            var image = element.GetComponent<Image>();
            if (image == null) continue;

            var hasConflict = map?.controllerMap.controllerType switch
            {
                ControllerType.Keyboard => conflicts.ContainsKey(map.keyboardKeyCode),
                ControllerType.Joystick => controllerConflicts.Any(x => x.controllerMap.id == map.controllerMap.id && x.id == map.id),
                _ => false
            };
            if (hasConflict)
            {
                image.color = Color.red;
            }
            else if (image.color == Color.red)
            {
                image.color = Color.white;
                element.ApplyTheme();
            }
        }
    }
}
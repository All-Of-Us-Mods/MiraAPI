using System.Linq;
using HarmonyLib;
using MiraAPI.Keybinds;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Rewired.UI.ControlMapper;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Patches.Keybinds;

[HarmonyPatch(typeof(ControlMapper))]
public static class ControlMapperPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ControlMapper.Start))]
    private static void StartPrefix(ControlMapper __instance)
    {
        var doneButton = GameObject.Find("DoneButton")?.GetComponent<CustomButton>();
        if (doneButton == null)
        {
            return;
        }

        var toggle = Object.Instantiate(doneButton, doneButton.transform.parent);
        var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
        var entry = PluginSingleton<MiraApiPlugin>.Instance!.MiraConfig!.ShowKeybinds;
        toggle.gameObject.name = "KeybindsVisibleToggle";
        text.text = $"Show Keybinds: {(entry.Value ? "On" : "Off")}";
        toggle.onClick.RemoveAllListeners();
        toggle.GetComponent<ButtonInfo>().identifier = "KeybindsVisibleToggle";
        toggle.onClick.AddListener((UnityAction)(() =>
        {
            entry.Value = !entry.Value;
            text.text = $"Show Keybinds: {(entry.Value ? "On" : "Off")}";
        }));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ControlMapper.Update))]
    private static void UpdatePrefix(ControlMapper __instance)
    {
        foreach (var element in __instance.themedElements)
        {
            var info = element.GetComponent<InputFieldInfo>();
            if (info == null)
            {
                continue;
            }

            var key = info.glyphOrText?.actionElementMap?.keyboardKeyCode;
            if (key == null)
            {
                continue;
            }

            var conflicts = KeybindManager.GetConflicts();
            if (conflicts.Any(x => x.Key.ToString() == key.ToString()))
            {
                element.GetComponent<Image>().color = Color.red;
            }
        }
    }
}

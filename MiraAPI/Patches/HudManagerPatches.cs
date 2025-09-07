using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Rewired;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches;

/// <summary>
/// General patches for the HudManager class.
/// </summary>
[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    // Custom buttons parent.
    public static GameObject? BottomLeft { get; private set; }
    public static Transform? BottomRight { get; private set; }
    public static Transform? Buttons { get; private set; }

    private static Dictionary<TextMeshPro, int> vanillaKeybindIcons = new();

    /*
    /// <summary>
    /// Trigger hudstart on current custom gamemode
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.OnGameStart))]
    public static void GameStartPatch(HudManager __instance)
    {
        CustomGameModeManager.ActiveMode?.HudStart(__instance);
    }*/

    /// <summary>
    /// Create custom buttons and arrange them on the hud.
    /// </summary>
    /// <param name="__instance">The HudManager instance.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Start))]
    public static void StartPostfix(HudManager __instance)
    {
        if (Buttons == null)
        {
            Buttons = __instance.transform.Find("Buttons");
        }

        if (BottomRight == null)
        {
            BottomRight = Buttons.Find("BottomRight");
        }

        if (BottomLeft == null)
        {
            BottomLeft = Object.Instantiate(BottomRight.gameObject, Buttons);
        }

        foreach (var t in BottomLeft.GetComponentsInChildren<ActionButton>(true))
        {
            t.gameObject.Destroy();
        }

        var gridArrange = BottomLeft.GetComponent<GridArrange>();
        var aspectPosition = BottomLeft.GetComponent<AspectPosition>();

        BottomLeft.name = "BottomLeft";
        gridArrange.Alignment = GridArrange.StartAlign.Right;
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            var location = button.Location switch
            {
                ButtonLocation.BottomLeft => BottomLeft.transform,
                ButtonLocation.BottomRight => BottomRight,
                _ => null,
            };

            if (location is null)
            {
                continue;
            }

            try
            {
                button.CreateButton(location);
            }
            catch (System.Exception e)
            {
                Logger<MiraApiPlugin>.Error($"Failed to create custom button {button.GetType().Name}: {e}");
            }
        }

        gridArrange.Start();
        gridArrange.ArrangeChilds();

        aspectPosition.AdjustPosition();

        vanillaKeybindIcons = [];
        var keybindIconPos = new Vector3(-0.4f, 0.4f, -9f);
        var vanillaButtons = new Dictionary<GameObject, int>
        {
            { __instance.KillButton.gameObject, 8 },
            { __instance.UseButton.gameObject, 6 },
            { __instance.ReportButton.gameObject, 7 },
            { __instance.ImpostorVentButton.gameObject, 50 },
            { __instance.SabotageButton.gameObject, 4 },
        };

        foreach (var kvp in vanillaButtons)
        {
            var buttonObj = kvp.Key;
            var actionId = kvp.Value;

            var key = KeybindUtils.GetKeycodeByActionId(actionId);
            if (key == KeyboardKeyCode.None)
            {
                continue;
            }
            var icon = Helpers.CreateKeybindIcon(buttonObj, key, keybindIconPos);
            vanillaKeybindIcons.Add(icon.transform.GetChild(0).GetComponent<TextMeshPro>(), actionId);
        }
    }

    /// <summary>
    /// Set the custom buttons active when the hud is active.
    /// </summary>
    /// <param name="__instance">HudManager instance.</param>
    /// <param name="localPlayer">The local PlayerControl.</param>
    /// <param name="role">The player's RoleBehaviour.</param>
    /// <param name="isActive">Whether the Hud should be set active or not.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void SetHudActivePostfix(HudManager __instance, PlayerControl localPlayer, RoleBehaviour role, bool isActive)
    {
        __instance.AdminButton.ToggleVisible(isActive && role.IsImpostor && GameOptionsManager.Instance.CurrentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek);
        if (localPlayer.Data == null)
        {
            return;
        }

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            try
            {
                button.SetActive(isActive, role);
            }
            catch (System.Exception e)
            {
                Logger<MiraApiPlugin>.Error($"Failed to set custom button {button.GetType().Name} active: {e}");
            }
        }
    }

    [HarmonyPatch(nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void UpdatePostfix()
    {
        foreach (var btnIcon in vanillaKeybindIcons)
        {
            btnIcon.Key.text = KeybindUtils.GetKeycodeByActionId(btnIcon.Value).ToString();
            btnIcon.Key.transform.parent.gameObject.SetActive(ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard &&
                                                              PluginSingleton<MiraApiPlugin>.Instance.MiraConfig!.ShowKeybinds.Value);
        }

        foreach (var entry in KeybindManager.Keybinds)
        {
            var player = ReInput.players.GetPlayer(0);
            var keyboard = player.controllers.Keyboard;

            bool modifierKeysPressed = entry.ModifierKeys.All(k => keyboard.GetModifierKey(k)) ||
                                       entry.ModifierKeys.Length <= 0;
            if (player.GetButtonDown(entry.Id) && modifierKeysPressed)
            {
                entry.Invoke();
            }
        }
    }
}

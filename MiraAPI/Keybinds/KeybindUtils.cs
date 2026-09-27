using System;
using System.Linq;
using MiraAPI.Hud;
using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Collection of keybind-related utilities.
/// </summary>
public static class KeybindUtils
{
    /// <summary>
    /// Gets the Rewired <see cref="InputManager_Base"/> instance.
    /// </summary>
    public static InputManager_Base? RewiredInputManager { get; internal set; }

    /// <summary>
    /// Returns the currently assigned keycode of a <see cref="BaseKeybind"/>.
    /// </summary>
    /// <param name="keybind">The <see cref="BaseKeybind"/> to get the keycode from.</param>
    /// <returns>The currently assigned keycode.</returns>
    public static KeyboardKeyCode GetKeycodeByKeybind(BaseKeybind keybind)
    {
        return keybind.RewiredInputAction == null ? KeyboardKeyCode.None : GetKeycodeByActionId(keybind.RewiredInputAction.id);
    }

    /// <summary>
    /// Returns the display label for the currently assigned controller button of a <see cref="BaseKeybind"/>.
    /// </summary>
    /// <param name="keybind">The keybind to get the controller button from.</param>
    /// <returns>The button label, or an empty string if no controller binding is available.</returns>
    public static string GetControllerButtonByKeybind(BaseKeybind keybind)
    {
        var map = keybind.RewiredInputAction == null ? null : GetControllerElementMap(keybind.RewiredInputAction.id);
        var name = map?.elementIdentifierName ?? string.Empty;
        return name.ToLowerInvariant() switch
        {
            "cross" => "X",
            "square" => "□",
            "circle" => "○",
            "triangle" => "△",
            _ => name,
        };
    }

    /// <summary>
    /// Gets the keycode for an action with <see cref="ReInput"/>.
    /// </summary>
    /// <param name="actionId">The action ID.</param>
    /// <returns>The keyboard keycode.</returns>
    public static KeyboardKeyCode GetKeycodeByActionId(int actionId)
    {
        return GetActionElementMap(actionId)?.keyboardKeyCode ?? KeyboardKeyCode.None;
    }

    /// <summary>
    /// Gets a <see cref="ActionElementMap"/> by id with <see cref="ReInput"/>.
    /// </summary>
    /// <param name="actionId">The action ID.</param>
    /// <returns>The keyboard binding, or null if the action is unassigned.</returns>
    public static ActionElementMap? GetActionElementMap(int actionId)
    {
        var player = ReInput.players.GetPlayer(0);
        return player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Keyboard, actionId, false);
    }

    /// <summary>
    /// Gets the binding for an action on the active controller.
    /// </summary>
    /// <param name="actionId">The action ID.</param>
    /// <returns>The controller binding, or null if no active controller or binding is available.</returns>
    public static ActionElementMap? GetControllerElementMap(int actionId)
    {
        var player = ReInput.players.GetPlayer(0);
        var controller = player.controllers.GetLastActiveController(ControllerType.Joystick);
        return controller == null ? null : player.controllers.maps.GetFirstElementMapWithAction(controller, actionId, false);
    }

    /// <summary>
    /// Gets an <see cref="InputAction"/> by id.
    /// </summary>
    /// <param name="id">The action ID.</param>
    /// <returns>The input action, or null if it is unavailable.</returns>
    public static InputAction? GetInputActionById(int id)
    {
        return RewiredInputManager?.userData?.GetActionById(id);
    }

    internal static bool IsControllerActionActive(int actionId)
    {
        var buttons = CustomButtonManager.Buttons.Where(x => x.Keybind?.RewiredInputAction?.id == actionId).ToArray();
        return buttons.Length == 0 || buttons.Any(x => x.Button != null && x.Button.isActiveAndEnabled && x.Enabled(PlayerControl.LocalPlayer?.Data?.Role));
    }

    /// <summary>
    /// Finds and returns an unused KeyCode that is not equal to the excluded key.
    /// </summary>
    /// <param name="exclude">The KeyCode to skip during the search.</param>
    /// <returns>
    /// The first available KeyCode not currently used by any registered keybind,
    /// or <see cref="KeyboardKeyCode.None"/> if none are available.
    /// </returns>
    public static KeyboardKeyCode FindAvailableKey(KeyboardKeyCode exclude)
    {
        foreach (var key in Enum.GetValues<KeyboardKeyCode>())
        {
            if (key == exclude) continue;
            var used = KeybindManager.Keybinds.Exists(e => e.DefaultKey == key);
            if (!used) return key;
        }

        return KeyboardKeyCode.None;
    }
}

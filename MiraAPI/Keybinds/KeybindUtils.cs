using System;
using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Collection of keybind-related utility classes.
/// </summary>
public static class KeybindUtils
{
    /// <summary>
    /// Gets the Rewired <see cref="InputManager_Base"/> instance.
    /// </summary>
    public static InputManager_Base? RewiredInputManager { get; internal set; }

    /// <summary>
    /// Returns the currently assigned keycode of a keybind.
    /// </summary>
    /// <param name="keybind">The keybind to get the keycode from.</param>
    /// <returns>The currently assigned keycode.</returns>
    public static KeyboardKeyCode? GetKeycodeByKeybind(MiraKeybind keybind)
    {
        if (keybind.RewiredInputAction == null)
        {
            return null;
        }

        return GetKeycodeByActionId(keybind.RewiredInputAction.id);
    }

    /// <summary>
    /// Gets the keycode for an action with ReInput.
    /// </summary>
    /// <param name="actionId">The action ID.</param>
    /// <returns>The keyboard keycode.</returns>
    public static KeyboardKeyCode GetKeycodeByActionId(int actionId)
    {
        var player = ReInput.players.GetPlayer(0);
        return player.controllers.maps.GetFirstElementMapWithAction(ControllerType.Keyboard, actionId, false)?
            .keyboardKeyCode ?? KeyboardKeyCode.None;
    }

    /// <summary>
    /// Finds and returns an unused KeyCode that is not equal to the excluded key.
    /// </summary>
    /// <param name="exclude">The KeyCode to skip during the search.</param>
    /// <returns>
    /// The first available KeyCode not currently used by any registered keybind,
    /// or KeyCode.None if none are available.
    /// </returns>
    public static KeyboardKeyCode FindAvailableKey(KeyboardKeyCode exclude)
    {
        foreach (KeyboardKeyCode key in Enum.GetValues<KeyboardKeyCode>())
        {
            if (key == exclude) continue;
            bool used = KeybindManager.Keybinds.Exists(e => e.DefaultKey == key);
            if (!used) return key;
        }
        return KeyboardKeyCode.None;
    }
}

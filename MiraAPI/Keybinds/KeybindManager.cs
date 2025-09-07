using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Lets you register and manage mod keybinds with conflict checks and rebinding support.
/// </summary>
public static class KeybindManager
{
    /// <summary>
    /// Gets a list of all registered keybinds.
    /// </summary>
    public static List<MiraKeybind> Keybinds { get; } = new();

    internal static void RewiredInit()
    {
        try
        {
            var instance = KeybindUtils.RewiredInputManager!;
            foreach (var keybind in KeybindManager.Keybinds)
            {
                if (instance.userData.actions.ToArray().Any(x => x.name == keybind.Id))
                {
                    Logger<MiraApiPlugin>.Warning($"Keybind of id {keybind.Id} already exists. Skipping it");
                    continue;
                }

                keybind.RewiredInputAction = instance.userData.RegisterModBind(
                    keybind.Id,
                    keybind.Name,
                    keybind.SourcePluginName,
                    keybind.DefaultKey,
                    modifiers: keybind.ModifierKeys);
            }
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Error($"Error while registering keybinds in Rewired: {e}");
        }
    }

    /// <summary>
    /// Returns all conflicts where keybinds use the same key.
    /// </summary>
    /// <returns>A list of pairs of keybinds that conflict.</returns>
    public static Dictionary<KeyboardKeyCode, List<MiraKeybind>> GetConflicts()
    {
        var conflicts = new Dictionary<KeyboardKeyCode, List<MiraKeybind>>();
        foreach (var keybind in Keybinds)
        {
            var all = GetKeybindsForKey(keybind.CurrentKey, true);
            if (all.Length > 1)
            {
                if (!conflicts.TryGetValue(keybind.CurrentKey, out var group))
                {
                    conflicts.Add(keybind.CurrentKey, new List<MiraKeybind>());
                    group = conflicts[keybind.CurrentKey];
                }

                if (!group.Contains(keybind))
                {
                    group.Add(keybind);
                }
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Returns all keybinds for a specified keycode.
    /// </summary>
    /// <param name="keyCode">The key to look for.</param>
    /// /// <param name="exclusiveCheck">Should only exlusive keybinds be present.</param>
    /// <returns>The list of keybinds.</returns>
    public static MiraKeybind[] GetKeybindsForKey(KeyboardKeyCode keyCode, bool exclusiveCheck = false)
    {
        var all = new List<MiraKeybind>();
        foreach (var keybind in Keybinds)
        {
            if (keybind.CurrentKey == keyCode && (keybind.Exclusive || !exclusiveCheck))
            {
                all.Add(keybind);
            }
        }

        return all.ToArray();
    }
}

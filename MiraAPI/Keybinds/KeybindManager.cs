using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities;
using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Lets you register and manage mod <see cref="MiraKeybind"/>s with conflict checks and rebinding support.
/// </summary>
public static class KeybindManager
{
    /// <summary>
    /// Gets a list of all registered <see cref="MiraKeybind"/>s.
    /// </summary>
    public static List<MiraKeybind> Keybinds { get; } = [];

    [HideFromIl2Cpp]
    internal static Dictionary<Type, VanillaKeybind> VanillaKeybinds { get; set; } = [];

    internal static void RewiredInit()
    {
        try
        {
            var instance = KeybindUtils.RewiredInputManager!;
            foreach (var keybind in Keybinds)
            {
                if (instance.userData.actions.ToArray().Any(x => x.name == keybind.Id))
                {
                    Warning($"Keybind of id {keybind.Id} already exists. Skipping it");
                    continue;
                }

                keybind.RewiredInputAction = instance.userData.RegisterModBind(keybind.Id, keybind.Name, keybind.SourcePluginName, keybind.DefaultKey, modifiers: keybind.ModifierKeys);

                if (keybind.DefaultControllerButton == null) continue;

                var button = (int)keybind.DefaultControllerButton.Value;
                var template = instance.dataFiles.GetJoystickTemplate(GamepadTemplate.typeGuid);
                foreach (var map in instance.userData.joystickMaps)
                {
                    if (map.categoryId != 1) continue;

                    var elementId = button;
                    var elementType = keybind.DefaultControllerButton is ControllerButton.LeftTrigger or ControllerButton.RightTrigger ? ControllerElementType.Axis : ControllerElementType.Button;

                    if (map.hardwareGuid != GamepadTemplate.typeGuid)
                    {
                        var controller = template?.joysticks.ToArray().FirstOrDefault(x => x.JoystickGuid == map.hardwareGuid);
                        elementId = controller?.GetJoystickElementId(button) ?? -1;
                        if (elementId < 0) continue;

                        var element = instance.dataFiles.GetHardwareJoystickMap(map.hardwareGuid)?.GetElementIdentifier(elementId);
                        if (element == null) continue;

                        elementType = element.elementType;
                    }

                    map.actionElementMaps.Add(new ActionElementMap
                    {
                        _actionId = keybind.RewiredInputAction.id,
                        _elementIdentifierId = elementId,
                        _elementType = elementType,
                        _axisRange = AxisRange.Positive,
                        _axisContribution = Pole.Positive,
                    });
                }
            }
        }
        catch (Exception e)
        {
            Error($"Error while registering keybinds in Rewired: {e}");
        }
    }

    /// <summary>
    /// Returns all conflicts where <see cref="MiraKeybind"/>s use the same key.
    /// </summary>
    /// <returns>The conflicting keybinds grouped by keycode.</returns>
    public static Dictionary<KeyboardKeyCode, List<MiraKeybind>> GetConflicts()
    {
        var conflicts = new Dictionary<KeyboardKeyCode, List<MiraKeybind>>();
        foreach (var keybind in Keybinds)
        {
            var key = keybind.CurrentKey;
            if (key == KeyboardKeyCode.None) continue;

            var all = GetKeybindsForKey(key, true);
            if (all.Length > 1)
            {
                if (!conflicts.TryGetValue(key, out var group))
                {
                    group = [];
                    conflicts.Add(key, group);
                }

                if (!group.Contains(keybind)) group.Add(keybind);
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Returns controller bindings shared by exclusive custom keybinds.
    /// </summary>
    /// <param name="enabledOnly">Whether to check only active controller maps and available HUD buttons.</param>
    /// <returns>The conflicting custom bindings.</returns>
    public static List<ActionElementMap> GetControllerConflicts(bool enabledOnly = false)
    {
        var conflicts = new List<ActionElementMap>();
        var actions = Keybinds.Where(x => x.Exclusive && x.RewiredInputAction != null && (!enabledOnly || KeybindUtils.IsControllerActionActive(x.RewiredInputAction.id))).Select(x => x.RewiredInputAction!.id).ToHashSet();
        var player = ReInput.players.GetPlayer(0);
        for (var i = 0; i < player.controllers.joystickCount; i++)
        {
            var controller = player.controllers.Joysticks[i];
            var maps = new CppCollections.List<ControllerMap>(player.controllers.maps.GetMaps(controller).Cast<CppCollections.IEnumerable<ControllerMap>>()).ToArray();
            foreach (var map in maps)
            {
                if (enabledOnly && !map.enabled) continue;

                foreach (var binding in map.GetElementMaps())
                {
                    if (!binding.enabled || !actions.Contains(binding.actionId)) continue;

                    foreach (var otherMap in maps)
                    {
                        if (enabledOnly && !otherMap.enabled) continue;
                        if (!enabledOnly && map.categoryId != otherMap.categoryId && map.categoryId != 0 && otherMap.categoryId != 0) continue;

                        foreach (var other in otherMap.GetElementMaps())
                        {
                            if (!other.enabled || binding.actionId == other.actionId || !actions.Contains(other.actionId) || !binding.CheckForAssignmentConflict(other)) continue;

                            if (!conflicts.Contains(binding)) conflicts.Add(binding);
                            if (!conflicts.Contains(other)) conflicts.Add(other);
                        }
                    }
                }
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Returns all <see cref="MiraKeybind"/>s for a specified keycode.
    /// </summary>
    /// <param name="keyCode">The key to look for.</param>
    /// <param name="exclusiveCheck">Whether to return only exclusive keybinds.</param>
    /// <returns>The list of <see cref="MiraKeybind"/>s.</returns>
    public static MiraKeybind[] GetKeybindsForKey(KeyboardKeyCode keyCode, bool exclusiveCheck = false)
    {
        var all = new List<MiraKeybind>();
        foreach (var keybind in Keybinds)
        {
            if (keybind.CurrentKey == keyCode && (keybind.Exclusive || !exclusiveCheck))
                all.Add(keybind);
        }

        return [.. all];
    }
}

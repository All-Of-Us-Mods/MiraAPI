using MiraAPI.Animations;
using System;
using System.Collections.Generic;

namespace MiraAPI.Modifiers;

/// <summary>
/// The manager for handling modifiers.
/// </summary>
public static class AnimationPresetManager
{
    private static uint _nextId;

    /// <summary>
    /// Gets registered Animation Presets.
    /// </summary>
    public static List<CustomAnimationPreset> Presets { get; internal set; } = new List<CustomAnimationPreset>();

    private static uint GetNextId()
    {
        _nextId++;
        return _nextId;
    }

    /// <summary>
    /// Get Animation Preset by ID.
    /// </summary>
    /// <param name="id">The id of the preset you want to find.</param>
    /// <returns>The animation preset.</returns>
    public static CustomAnimationPreset? GetPresetById(uint id)
    {
        return Presets.Find(p => p.Id == id);
    }

    internal static void RegisterPreset(Type modifierType)
    {
        if (!typeof(CustomAnimationPreset).IsAssignableFrom(modifierType))
        {
            return;
        }

        CustomAnimationPreset preset = (CustomAnimationPreset)Activator.CreateInstance(modifierType);
        preset.Id = GetNextId();
        Presets.Add(preset);
    }
}

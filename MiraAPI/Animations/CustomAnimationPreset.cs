using UnityEngine;

namespace MiraAPI.Animations;

/// <summary>
/// Configure an animation's settings using presets.
/// </summary>
/// <param name="clip">The animation clip the preset will use.</param>
public abstract class CustomAnimationPreset(AnimationClip clip)
{
    internal uint Id;

    internal void PlayAnimation(PlayerControl source)
    {

    }

    /// <summary>
    /// Check whether the local player should be able to see this animation.
    /// </summary>
    /// <param name="source">The player who the animation is being triggered upon.</param>
    /// <returns>Whether the local player can see the animation or not.</returns>
    public abstract bool AnimationVisible(PlayerControl source);
}
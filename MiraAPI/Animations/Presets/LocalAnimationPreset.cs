using UnityEngine;

namespace MiraAPI.Animations.Presets;
public class LocalAnimationPreset(AnimationClip clip) : CustomAnimationPreset(clip)
{
    /// <inheritdoc/>
    public override bool AnimationVisible(PlayerControl source)
    {
        return source.AmOwner;
    }
}

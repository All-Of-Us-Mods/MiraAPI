using MiraAPI.Animations.Presets;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Attributes;
using System;
using UnityEngine;

namespace MiraAPI.Animations;

[RegisterInIl2Cpp]
public class CustomAnimator(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private PlayerControl _player;

    private void Start()
    {
        _player = gameObject.GetComponent<PlayerControl>();

        _player.GetCustomAnimator()?.TriggerCustomAnimation(new LocalAnimationPreset(new AnimationClip()));
    }

    public void TriggerCustomAnimation(CustomAnimationPreset preset) => RpcTriggerCustomAnimation(_player, preset.Id);


    [MethodRpc((uint)MiraRpc.TriggerAnimation)]
    internal static void RpcTriggerCustomAnimation(PlayerControl source, uint presetId)
    {
        CustomAnimationPreset preset = AnimationPresetManager.GetPresetById(presetId);
        if (preset is null) return;

        if (preset.AnimationVisible(source))
        {
            preset.PlayAnimation(source);
        }
    }
}
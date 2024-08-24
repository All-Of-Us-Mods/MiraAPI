using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiraAPI.Modifiers;

[RegisterInIl2Cpp]
public class ModifierComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public readonly List<BaseModifier> ActiveModifiers = [];

    public PlayerControl player;

    public void Start()
    {
        player = GetComponent<PlayerControl>();
    }

    public void Update()
    {
        foreach (var modifier in ActiveModifiers)
        {
            modifier.Update();
        }
    }

    public static void RemoveModifier(PlayerControl target, uint modifierId)
    {
        if (!ModifierManager.IdToTypeModifiers.TryGetValue(modifierId, out var type))
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier with id {modifierId} because it is not registered.");
            return;
        }

        var modifierComponent = target.GetModifierComponent();

        var modifier = modifierComponent.ActiveModifiers.Find(x => x.GetType() == type);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {type.Name} because it is not active.");
            return;
        }

        modifier.OnDeactivate();
        modifierComponent.ActiveModifiers.Remove(modifier);
    }

    public static void AddModifier(PlayerControl target, uint modifierId)
    {
        if (!ModifierManager.IdToTypeModifiers.TryGetValue(modifierId, out var type))
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier with id {modifierId} because it is not registered.");
            return;
        }

        var modifier = (BaseModifier)Activator.CreateInstance(type);

        var modifierComponent = target.GetModifierComponent();

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {type.Name} because it is null.");
            return;
        }

        modifierComponent.ActiveModifiers.Add(modifier);
        modifier.Player = modifierComponent.player;
        modifier.OnActivate();

        if (modifier is TimedModifier { AutoStart: true } timer && target.AmOwner)
        {
            timer.StartTimer();
        }
    }

    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(PlayerControl target, uint modifierId) => RemoveModifier(target, modifierId);

    [MethodRpc((uint)MiraRpc.AddModifier)]
    public static void RpcAddModifier(PlayerControl target, uint modifierId) => AddModifier(target, modifierId);
}
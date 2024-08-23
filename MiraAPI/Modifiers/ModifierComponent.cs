using Reactor.Utilities.Attributes;
using System;
using System.Collections.Generic;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Modifiers;

[RegisterInIl2Cpp]
public class ModifierComponent(IntPtr ptr) : MonoBehaviour(ptr)
{
    public List<BaseModifier> activeModifiers = [];

    public PlayerControl player;

    public void Start()
    {
        player = GetComponent<PlayerControl>();
    }

    public bool HasModifier<T>() where T : BaseModifier
    {
        return activeModifiers.Exists(x=>x is T);
    }
    
    public void AddModifier<T>() where T : BaseModifier
    {
        if (!ModifierManager.TypeToIdModifiers.TryGetValue(typeof(T), out var id))
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }
        
        RpcAddModifier(player, id);
    }

    public void RemoveModifier<T>() where T : BaseModifier
    {
        if (!ModifierManager.TypeToIdModifiers.TryGetValue(typeof(T), out var id))
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }
        
        RpcRemoveModifier(player, id);
    }

    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    private static void RpcRemoveModifier(PlayerControl target, uint modifierId)
    {
        if (!ModifierManager.IdToTypeModifiers.TryGetValue(modifierId, out var type))
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier with id {modifierId} because it is not registered.");
            return;
        }
        
        var modifierComponent = target.GetModifierComponent();
        
        var modifier = modifierComponent.activeModifiers.Find(x => x.GetType() == type);

        if (modifier is null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot remove modifier {type.Name} because it is not active.");
            return;
        }
        
        modifier.OnDeactivate();
        modifierComponent.activeModifiers.Remove(modifier);
    }

    [MethodRpc((uint)MiraRpc.AddModifier)]
    private static void RpcAddModifier(PlayerControl target, uint modifierId)
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
        
        modifierComponent.activeModifiers.Add(modifier);
        modifier.Player = modifierComponent.player; 
        modifier.OnActivate();
    }
}
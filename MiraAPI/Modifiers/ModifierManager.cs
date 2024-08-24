using MiraAPI.Networking;
using System;
using System.Collections.Generic;
using MiraAPI.Utilities;
using Reactor.Networking.Rpc;

namespace MiraAPI.Modifiers;

public static class ModifierManager
{
    public static readonly Dictionary<uint, Type> IdToTypeModifiers = [];
    public static readonly Dictionary<Type, uint> TypeToIdModifiers = [];

    private static uint _nextId;

    private static uint GetNextId()
    {
        _nextId++;
        return _nextId;
    }

    public static void RegisterModifier(Type modifierType)
    {
        if (!typeof(BaseModifier).IsAssignableFrom(modifierType))
        {
            return;
        }

        IdToTypeModifiers.Add(GetNextId(), modifierType);
        TypeToIdModifiers.Add(modifierType, _nextId);
    }

    public static void SyncAllModifiers(int targetId = -1)
    {
        var data = new List<NetData>();
        
        if (targetId == -1)
        {
            foreach (var player in GameData.Instance.AllPlayers)
            {
                data.Add(GetPlayerModifiers(player.Object));
            }
        }
        else
        {
            var player = GameData.Instance.GetPlayerById((byte)targetId).Object;
            
            data.Add(GetPlayerModifiers(player));
        }
        
        
        Rpc<SyncModifiersRpc>.Instance.Send(PlayerControl.LocalPlayer, data.ToArray(), true);
    }
    
    public static void HandleSyncModifiers(NetData[] data)
    {
        foreach (var netData in data)
        {
            var ids = new uint[netData.Data.Length / 4];
            Buffer.BlockCopy(netData.Data, 0, ids, 0, netData.Data.Length);
            
            var plr = GameData.Instance.GetPlayerById((byte)netData.Id).Object;
            var modifierComponent = plr.GetComponent<ModifierComponent>();
            
            if (!modifierComponent)
            {
                continue;
            }

            foreach (var modifier in modifierComponent.ActiveModifiers)
            {
                modifier.OnDeactivate();
            }
            modifierComponent.ActiveModifiers.Clear();
            
            foreach (var id in ids)
            {
                ModifierComponent.AddModifier(plr, id);
            }
        }
    }

    private static NetData GetPlayerModifiers(PlayerControl player)
    {
        var bytes = new List<byte>();
        var modifierComponent = player.GetComponent<ModifierComponent>();
        if (!modifierComponent)
        {
            return new NetData(player.PlayerId, []);
        }
                
        foreach (var modifier in modifierComponent.ActiveModifiers)
        {
            bytes.AddRange(BitConverter.GetBytes(TypeToIdModifiers[modifier.GetType()]));
        }
        
        return new NetData(player.PlayerId, bytes.ToArray());
    }
}
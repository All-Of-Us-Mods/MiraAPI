using MiraAPI.Networking;
using System;
using System.Collections.Generic;

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

    }

    public static void HandleSyncModifiers(NetData[] data)
    {
        foreach (var netData in data)
        {
            PlayerControl plr = GameData.Instance.GetPlayerById((byte)netData.Id).Object;
            foreach (var id in netData.Data)
            {
                ModifierComponent.AddModifier(plr, id);
            }
        }
    }
}
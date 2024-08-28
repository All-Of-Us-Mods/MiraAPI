using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

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

    internal static void RegisterModifier(Type modifierType)
    {
        if (!typeof(BaseModifier).IsAssignableFrom(modifierType))
        {
            return;
        }

        IdToTypeModifiers.Add(GetNextId(), modifierType);
        TypeToIdModifiers.Add(modifierType, _nextId);
    }

    internal static void AssignModifiers(List<PlayerControl> plrs)
    {
        var rand = new Random();

        List<uint> filteredModifiers = [];

        foreach (var modifier in IdToTypeModifiers.Where(pair => pair.Value.GetType().IsAssignableTo(typeof(GameModifier))))
        {
            var mod = (GameModifier)Activator.CreateInstance(modifier.Value);
            var num = mod.GetAmountPerGame();
            var chance = mod.GetAssignmentChance();

            for (var i = 0; i < num; i++)
            {
                var randomNum = rand.Next(100);

                if (randomNum < Math.Clamp(chance, 0, 100))
                {
                    filteredModifiers.Add(TypeToIdModifiers[modifier.Value]);
                }
            }
        }

        var shuffledModifiers = filteredModifiers.Randomize();
        if (shuffledModifiers.Count > plrs.Count)
        {
            shuffledModifiers = shuffledModifiers.GetRange(0, plrs.Count);
        }

        while (shuffledModifiers.Count > 0)
        {
            var id = shuffledModifiers[0];

            var mod = (GameModifier)Activator.CreateInstance(IdToTypeModifiers[id]);
            var plr = plrs.Random();

            if (plr.Data.Role is ICustomRole modRole)
            {
                if (!modRole.IsModifierApplicable(mod))
                {
                    continue;
                }
            }

            if (plr.HasModifier(id) || mod.IsModifierValidOn(plr.Data.Role))
            {
                continue;
            }

            shuffledModifiers.RemoveAt(0);
            ModifierComponent.RpcAddModifier(plr, id);
        }
    }

    internal static void SyncAllModifiers(int targetId = -1)
    {
        var data = new List<NetData>();

        foreach (var player in GameData.Instance.AllPlayers)
        {
            data.Add(GetPlayerModifiers(player.Object));
        }

        Rpc<SyncModifiersRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, data.ToArray());
    }

    internal static void HandleSyncModifiers(NetData[] data)
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
using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Random = System.Random;

namespace MiraAPI.Modifiers;

/// <summary>
/// The manager for handling modifiers.
/// </summary>
public static class ModifierManager
{
    private static readonly Dictionary<uint, Type> IdToTypeModifierMap = [];
    private static readonly Dictionary<Type, uint> TypeToIdModifierMap = [];
    private static readonly Dictionary<int, List<Type>> PrioritiesToTypesMap = [];

    private static uint _nextId;

    private static uint GetNextId()
    {
        _nextId++;
        return _nextId;
    }

    /// <summary>
    /// Gets the modifier type from the id.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>The Type of the modifier.</returns>
    public static Type? GetModifierType(uint id)
    {
        return IdToTypeModifierMap.GetValueOrDefault(id);
    }

    /// <summary>
    /// Gets the modifier id from the type.
    /// </summary>
    /// <param name="type">The Type.</param>
    /// <returns>The ID of the modifier.</returns>
    public static uint? GetModifierId(Type type)
    {
        return TypeToIdModifierMap.GetValueOrDefault(type);
    }

    internal static void RegisterModifier(Type modifierType)
    {
        if (!typeof(BaseModifier).IsAssignableFrom(modifierType))
        {
            return;
        }

        IdToTypeModifierMap.Add(GetNextId(), modifierType);
        TypeToIdModifierMap.Add(modifierType, _nextId);

        if (!typeof(GameModifier).IsAssignableFrom(modifierType))
        {
            return;
        }

        var mod = Activator.CreateInstance(modifierType) as GameModifier;
        var prio = mod!.Priority();

        if (!PrioritiesToTypesMap.TryGetValue(prio, out var list))
        {
            PrioritiesToTypesMap[prio] = list = [];
        }

        list.Add(modifierType);
    }

    internal static void AssignModifiers(List<PlayerControl> plrs)
    {
        var rand = new Random();

        Dictionary<uint, int> filteredModifiers = [];

        foreach (var prioPairs in PrioritiesToTypesMap)
        {
            foreach (var modifier in prioPairs.Value)
            {
                var mod = Activator.CreateInstance(modifier) as GameModifier;
                var id = TypeToIdModifierMap[modifier];

                if (!plrs.Exists(x => IsGameModifierValid(x, mod!, id)))
                {
                    Logger<MiraApiPlugin>.Warning("No players are valid for modifier: " + mod!.ModifierName);
                    continue;
                }

                var maxCount = plrs.Count(x => IsGameModifierValid(x, mod!, id));
                var num = Math.Clamp(mod!.GetAmountPerGame(), 0, maxCount);
                var chance = mod.GetAssignmentChance();

                for (var i = 0; i < num; i++)
                {
                    var randomNum = rand.Next(100);

                    if (randomNum < Math.Clamp(chance, 0, 100))
                    {
                        filteredModifiers.Add(id, prioPairs.Key);
                    }
                }
            }
        }

        var shuffledModifiers = filteredModifiers.ToList().Randomize().OrderByDescending(x => x.Value).Select(x => x.Key).ToList();

        if (shuffledModifiers.Count > plrs.Count)
        {
            shuffledModifiers = shuffledModifiers.GetRange(0, plrs.Count);
        }

        while (shuffledModifiers.Count > 0)
        {
            var id = shuffledModifiers[0];

            if (Activator.CreateInstance(IdToTypeModifierMap[id]) is not GameModifier mod)
            {
                Logger<MiraApiPlugin>.Error($"Failed to create instance of {IdToTypeModifierMap[id].Name}");
                continue;
            }

            if (!plrs.Exists(x => IsGameModifierValid(x, mod, id)))
            {
                shuffledModifiers.RemoveAt(0);
                continue;
            }

            var plr = plrs.Random();

            if (plr == null || !IsGameModifierValid(plr, mod, id))
            {
                continue;
            }

            shuffledModifiers.RemoveAt(0);
            plr.RpcAddModifier(id);
        }
    }

    private static bool IsGameModifierValid(PlayerControl player, GameModifier modifier, uint modifierId)
    {
        return (player.Data.Role is not ICustomRole role || role.IsModifierApplicable(modifier)) &&
               modifier.IsModifierValidOn(player.Data.Role) &&
               !player.HasModifier(modifierId);
    }

    internal static void SyncAllModifiers(int targetId = -1)
    {
        var data = new List<NetData>();

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            data.Add(GetPlayerModifiers(player));
        }

        Rpc<SyncModifiersRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, [.. data]);
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

            modifierComponent.ClearModifiers();

            foreach (var id in ids)
            {
                if (!IdToTypeModifierMap.TryGetValue(id, out var type))
                {
                    Logger<MiraApiPlugin>.Error($"Cannot add modifier with id {id} because it is not registered.");
                    continue;
                }

                modifierComponent.AddModifier(type);
            }
        }
    }

    private static NetData GetPlayerModifiers(PlayerControl? player)
    {
        if (player == null || !player)
        {
            return new NetData(0, []);
        }

        List<byte> bytes = [];
        var modifierComponent = player.GetComponent<ModifierComponent>();
        if (modifierComponent == null || !modifierComponent)
        {
            return new NetData(player.PlayerId, []);
        }

        foreach (var modifier in modifierComponent.ActiveModifiers)
        {
            bytes.AddRange(BitConverter.GetBytes(TypeToIdModifierMap[modifier.GetType()]));
        }

        return new NetData(player.PlayerId, [.. bytes]);
    }
}

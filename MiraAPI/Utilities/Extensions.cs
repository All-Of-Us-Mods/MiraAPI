#nullable enable
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiraAPI.Utilities;

public static class Extensions
{
    public static bool IsStatic(this System.Type type)
    {
        return type is { IsClass: true, IsAbstract: true, IsSealed: true };
    }

    public static Color32 GetShadowColor(this Color32 color, byte darknessAmount)
    {
        return
            new Color32((byte)Mathf.Clamp(color.r - darknessAmount, 0, 255), (byte)Mathf.Clamp(color.g - darknessAmount, 0, 255),
                (byte)Mathf.Clamp(color.b - darknessAmount, 0, 255), byte.MaxValue);
    }

    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }

    public static Queue<NetData[]> ChunkNetData(this IEnumerable<NetData> dataCollection, int chunkSize)
    {
        Queue<NetData[]> chunks = [];
        List<NetData> current = [];

        var count = 0;
        foreach (var netData in dataCollection)
        {
            var length = netData.GetLength();

            if (length > chunkSize)
            {
                Logger<MiraApiPlugin>.Error($"NetData length is greater than chunk size: {length} > {chunkSize}");
                continue;
            }

            if (count + length > chunkSize)
            {
                chunks.Enqueue(current.ToArray());
                current.Clear();
                count = 0;
            }

            current.Add(netData);
        }

        if (current.Count > 0)
        {
            chunks.Enqueue(current.ToArray());
        }

        return chunks;
    }


    public static bool IsCustom(this OptionBehaviour optionBehaviour)
    {
        return ModdedOptionsManager.ModdedOptions.Values.Any(opt => opt.OptionBehaviour && opt.OptionBehaviour.Equals(optionBehaviour));
    }

    public static readonly Dictionary<PlayerControl, ModifierComponent> ModifierComponents = new();

    public static ModifierComponent? GetModifierComponent(this PlayerControl player)
    {
        if (ModifierComponents.TryGetValue(player, out var component))
        {
            return component;
        }

        component = player.GetComponent<ModifierComponent>();
        if (component)
        {
            ModifierComponents[player] = component;
        }

        return component;
    }

    public static List<T> Randomize<T>(this List<T> list)
    {
        List<T> randomizedList = [];
        System.Random rnd = new();
        while (list.Count > 0)
        {
            var index = rnd.Next(0, list.Count);
            randomizedList.Add(list[index]);
            list.RemoveAt(index);
        }
        return randomizedList;
    }

    public static T? GetModifier<T>(this PlayerControl? player) where T : BaseModifier
    {
        return player?.GetModifierComponent()?.ActiveModifiers.Find(x => x is T) as T;
    }

    public static bool HasModifier<T>(this PlayerControl? player) where T : BaseModifier
    {
        return player?.GetModifierComponent() != null && player.GetModifierComponent()!.ActiveModifiers.Exists(x => x is T);
    }

    public static bool HasModifier(this PlayerControl? player, uint id)
    {
        return player?.GetModifierComponent() != null && player.GetModifierComponent()!.ActiveModifiers.Exists(x => x.ModifierId == id);
    }

    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(this PlayerControl target, uint modifierId)
    {
        target.GetModifierComponent()?.RemoveModifier(modifierId);
    }

    [MethodRpc((uint)MiraRpc.AddModifier)]
    public static BaseModifier? RpcAddModifier(this PlayerControl target, uint modifierId)
    {
        if (ModifierManager.IdToTypeModifiers.TryGetValue(modifierId, out var type))
        {
            return target.GetModifierComponent()?.AddModifier(type);
        }

        Logger<MiraApiPlugin>.Error($"Cannot add modifier with id {modifierId} because it is not registered.");
        return null;
    }

    public static void RpcAddModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        if (!ModifierManager.TypeToIdModifiers.TryGetValue(typeof(T), out var id))
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }

        player.RpcAddModifier(id);
    }

    public static void RpcRemoveModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        if (!ModifierManager.TypeToIdModifiers.TryGetValue(typeof(T), out var id))
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }

        player.RpcAddModifier(id);
    }

    public static Color DarkenColor(this Color color, float amount = 0.45f)
    {
        return new Color(color.r - amount, color.g - amount, color.b - amount);
    }
    public static Color GetAlternateColor(this Color color, float amount = 0.45f)
    {
        return color.IsColorDark() ? LightenColor(color, amount) : DarkenColor(color, amount);
    }

    public static Color LightenColor(this Color color, float amount = 0.45f)
    {
        return new Color(color.r + amount, color.g + amount, color.b + amount);
    }

    public static bool IsColorDark(this Color color)
    {
        return color.r < 0.5f && color is { g: < 0.5f, b: < 0.5f };
    }

    public static DeadBody? GetNearestDeadBody(this PlayerControl playerControl, float radius)
    {
        return Helpers.GetNearestDeadBodies(playerControl.GetTruePosition(), radius).FirstOrDefault(component => component && !component.Reported);
    }

    public static T? GetNearestObjectOfType<T>(this PlayerControl playerControl, float radius, string? colliderTag = null, System.Func<T, bool>? predicate = null) where T : Component
    {
        return Helpers.GetNearestObjectsOfType<T>(playerControl.GetTruePosition(), radius, colliderTag).FirstOrDefault(predicate ?? (component => component));
    }

    public static PlayerControl? GetClosestPlayer(this PlayerControl playerControl, bool includeImpostors, float distance, bool ignoreColliders = false)
    {
        if (!ShipStatus.Instance)
        {
            return null;
        }

        var filteredPlayers = Helpers.GetClosestPlayers(playerControl, distance, ignoreColliders)
            .Where(playerInfo => !playerInfo.Data.Disconnected && playerInfo.PlayerId != playerControl.PlayerId && !playerInfo.Data.IsDead &&
                                 (includeImpostors || !playerInfo.Data.Role.IsImpostor));

        return filteredPlayers.First();
    }
}
#nullable enable
using System;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using System.Linq;
using MiraAPI.Networking;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Utilities;

public static class Extensions
{
    public static bool IsStatic(this Type type)
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

    public static Color DarkenColor(this Color color)
    {
        return new Color(color.r - 0.45f, color.g - 0.45f, color.b - 0.45f);
    }
    public static Color GetAlternateColor(this Color color)
    {
        return color.IsColorDark() ? LightenColor(color) : DarkenColor(color);
    }

    public static Color LightenColor(this Color color)
    {
        return new Color(color.r + 0.45f, color.g + 0.45f, color.b + 0.45f);
    }

    public static bool IsColorDark(this Color color)
    {
        return color.r < 0.5f && color is { g: < 0.5f, b: < 0.5f };
    }

    public static DeadBody? NearestDeadBody(this PlayerControl playerControl, float radius)
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(playerControl.GetTruePosition(), radius, Helpers.Filter, results);
        return results.ToArray()
            .Where(collider2D => collider2D.CompareTag("DeadBody"))
            .Select(collider2D => collider2D.GetComponent<DeadBody>())
            .FirstOrDefault(component => component && !component.Reported);
    }

    public static T? GetNearestObjectOfType<T>(this PlayerControl playerControl, float radius, string? colliderTag=null, Func<T, bool>? predicate=null) where T : Component
    {
        var results = new Il2CppSystem.Collections.Generic.List<Collider2D>();
        Physics2D.OverlapCircle(playerControl.GetTruePosition(), radius, Helpers.Filter, results);
        return results.ToArray()
            .Where(collider2D => colliderTag == null || collider2D.CompareTag(colliderTag))
            .Select(collider2D => collider2D.GetComponent<T>())
            .FirstOrDefault(predicate ?? (component => component));
    }

    public static PlayerControl? GetClosestPlayer(this PlayerControl playerControl, bool includeImpostors, float distance)
    {
        PlayerControl? result = null;
        if (!ShipStatus.Instance)
        {
            return null;
        }

        var truePosition = playerControl.GetTruePosition();

        var filteredPlayers = GameData.Instance.AllPlayers.ToArray()
            .Where(playerInfo => !playerInfo.Disconnected && playerInfo.PlayerId != playerControl.PlayerId && !playerInfo.IsDead &&
                                 (includeImpostors || !playerInfo.Role.IsImpostor));
        
        foreach (var playerInfo in filteredPlayers)
        {
            var @object = playerInfo.Object;
            if (!@object)
            {
                continue;
            }

            var vector = @object.GetTruePosition() - truePosition;
            var magnitude = vector.magnitude;
            if (!(magnitude <= distance) || PhysicsHelpers.AnyNonTriggersBetween(truePosition,
                    vector.normalized,
                    magnitude, LayerMask.GetMask("Ship", "Objects")))
            {
                continue;
            }

            result = @object;
            distance = magnitude;
        }
        return result;
    }
}
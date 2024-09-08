using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Utilities;

/// <summary>
/// Extension methods for various classes.
/// </summary>
public static class Extensions
{
    internal static NetData GetNetData(this ICustomRole role)
    {
        role.ParentMod.PluginConfig.TryGetEntry<int>(role.NumConfigDefinition, out var numEntry);
        role.ParentMod.PluginConfig.TryGetEntry<int>(role.ChanceConfigDefinition, out var chanceEntry);

        return new NetData(RoleId.Get(role.GetType()), BitConverter.GetBytes(numEntry.Value).AddRangeToArray(BitConverter.GetBytes(chanceEntry.Value)));
    }

    /// <summary>
    /// Checks if a type is static.
    /// </summary>
    /// <param name="type">The type being checked.</param>
    /// <returns>True if the type is static, false otherwise.</returns>
    public static bool IsStatic(this Type type)
    {
        return type is { IsClass: true, IsAbstract: true, IsSealed: true };
    }

    /// <summary>
    /// Gets a darkened version of a color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="darknessAmount">A darkness amount between 0 and 255.</param>
    /// <returns>The darkened color.</returns>
    public static Color32 GetShadowColor(this Color32 color, byte darknessAmount)
    {
        return
            new Color32(
                (byte)Mathf.Clamp(color.r - darknessAmount, 0, 255),
                (byte)Mathf.Clamp(color.g - darknessAmount, 0, 255),
                (byte)Mathf.Clamp(color.b - darknessAmount, 0, 255),
                byte.MaxValue);
    }

    /// <summary>
    /// Truncates a string to a specified length.
    /// </summary>
    /// <param name="value">The original string.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="truncationSuffix">An option suffix to attach at the end of the truncated string.</param>
    /// <returns>A truncated string of maxLength with the attached suffix.</returns>
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }

    /// <summary>
    /// Chunks a collection of NetData into smaller arrays.
    /// </summary>
    /// <param name="dataCollection">A collection of NetData objects.</param>
    /// <param name="chunkSize">The max chunk size in bytes.</param>
    /// <returns>A Queue of NetData arrays.</returns>
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
            chunks.Enqueue([.. current]);
        }

        return chunks;
    }

    /// <summary>
    /// Determines if a given OptionBehaviour is for a custom option.
    /// </summary>
    /// <param name="optionBehaviour">The OptionBehaviour to be tested.</param>
    /// <returns>True if the OptionBehaviour is for a custom options, false otherwise.</returns>
    public static bool IsCustom(this OptionBehaviour optionBehaviour)
    {
        return ModdedOptionsManager.ModdedOptions.Values.Any(opt => opt.OptionBehaviour && opt.OptionBehaviour == optionBehaviour);
    }

    private static readonly Dictionary<PlayerControl, ModifierComponent> ModifierComponents = [];

    /// <summary>
    /// Gets the ModifierComponent for a player.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <returns>A ModifierComponent if there is one, null otherwise.</returns>
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

    /// <summary>
    /// Randomizes a list.
    /// </summary>
    /// <param name="list">The list object.</param>
    /// <typeparam name="T">The type of object the list contains.</typeparam>
    /// <returns>A randomized list made from the original list.</returns>
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

    /// <summary>
    /// Gets a modifier by its type, or null if the player doesn't have it.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>The Modifier if it is found, null otherwise.</returns>
    public static T? GetModifier<T>(this PlayerControl? player) where T : BaseModifier
    {
        return player?.GetModifierComponent()?.ActiveModifiers.Find(x => x is T) as T;
    }

    /// <summary>
    /// Checks if a player has a modifier.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    public static bool HasModifier<T>(this PlayerControl? player) where T : BaseModifier
    {
        return player?.GetModifierComponent() != null && player.GetModifierComponent()!.ActiveModifiers.Exists(x => x is T);
    }

    /// <summary>
    /// Checks if a player has a modifier by its ID.
    /// </summary>
    /// <param name="player">The PlayerControl object.</param>
    /// <param name="id">The Modifier ID.</param>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    public static bool HasModifier(this PlayerControl? player, uint id)
    {
        return player?.GetModifierComponent() != null && player.GetModifierComponent()!.ActiveModifiers.Exists(x => x.ModifierId == id);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="target">The player to remove the modifier from.</param>
    /// <param name="modifierId">The ID of the modifier.</param>
    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(this PlayerControl target, uint modifierId)
    {
        target.GetModifierComponent()?.RemoveModifier(modifierId);
    }

    /// <summary>
    /// Remote Procedure Call to remove a modifier from a player.
    /// </summary>
    /// <param name="player">The player to remove the modifier from.</param>
    /// <typeparam name="T">The Type of the Modifier.</typeparam>
    public static void RpcRemoveModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        var id = ModifierManager.GetModifierId(typeof(T));

        if (id == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }

        player.RpcRemoveModifier(id.Value);
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="target">The player to add the modifier to.</param>
    /// <param name="modifierId">The modifier ID.</param>
    [MethodRpc((uint)MiraRpc.AddModifier)]
    public static void RpcAddModifier(this PlayerControl target, uint modifierId)
    {
        var type = ModifierManager.GetModifierType(modifierId);
        if (type == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier with id {modifierId} because it is not registered.");
            return;
        }
        target.GetModifierComponent()?.AddModifier(type);
    }

    /// <summary>
    /// Remote Procedure Call to add a modifier to a player.
    /// </summary>
    /// <param name="player">The player to add the modifier to.</param>
    /// <typeparam name="T">The modifier Type.</typeparam>
    public static void RpcAddModifier<T>(this PlayerControl player) where T : BaseModifier
    {
        var id = ModifierManager.GetModifierId(typeof(T));
        if (id == null)
        {
            Logger<MiraApiPlugin>.Error($"Cannot add modifier {typeof(T).Name} because it is not registered.");
            return;
        }

        player.RpcAddModifier(id.Value);
    }

    /// <summary>
    /// Darkens a color by a specified amount.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="amount">A float amount between 0 and 1.</param>
    /// <returns>The darkened color.</returns>
    public static Color DarkenColor(this Color color, float amount = 0.45f)
    {
        return new Color(color.r - amount, color.g - amount, color.b - amount);
    }

    /// <summary>
    /// Gets an alternate color based on the original color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="amount">The amount to darken or lighten the original color by between 0.0 and 1.0.</param>
    /// <returns>An alternate color that has been darkened or lightened.</returns>
    public static Color GetAlternateColor(this Color color, float amount = 0.45f)
    {
        return color.IsColorDark() ? LightenColor(color, amount) : DarkenColor(color, amount);
    }

    /// <summary>
    /// Lightens a color by a specified amount.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="amount">A float amount between 0.0 and 1.0.</param>
    /// <returns>The lightened color.</returns>
    public static Color LightenColor(this Color color, float amount = 0.45f)
    {
        return new Color(color.r + amount, color.g + amount, color.b + amount);
    }

    /// <summary>
    /// Checks if a color is dark.
    /// </summary>
    /// <param name="color">The color to check.</param>
    /// <returns>True if the color is dark, false otherwise.</returns>
    public static bool IsColorDark(this Color color)
    {
        return color.r < 0.5f && color is { g: < 0.5f, b: < 0.5f };
    }

    /// <summary>
    /// Gets the nearest dead body to a player.
    /// </summary>
    /// <param name="playerControl">The player object.</param>
    /// <param name="radius">The radius to search within.</param>
    /// <returns>The dead body if it is found, or null there is none within the radius.</returns>
    public static DeadBody? GetNearestDeadBody(this PlayerControl playerControl, float radius)
    {
        return Helpers.GetNearestDeadBodies(playerControl.GetTruePosition(), radius, Helpers.CreateFilter(Constants.NotShipMask)).Find(component => component && !component.Reported);
    }

    /// <summary>
    /// Finds the nearest object of a specified type to a player. It will only work if the object has a collider.
    /// </summary>
    /// <param name="playerControl">The player object.</param>
    /// <param name="radius">The radius to search within.</param>
    /// <param name="filter">The contact filter.</param>
    /// <param name="colliderTag">An optional collider tag.</param>
    /// <param name="predicate">Optional predicate to test if the object is valid.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The object if it was found, or null if there is none within the radius.</returns>
    public static T? GetNearestObjectOfType<T>(this PlayerControl playerControl, float radius, ContactFilter2D filter, string? colliderTag = null, Predicate<T>? predicate = null) where T : Component
    {
        return Helpers.GetNearestObjectsOfType<T>(playerControl.GetTruePosition(), radius, filter, colliderTag).Find(predicate ?? (component => component));
    }

    /// <summary>
    /// Gets the nearest player to a player.
    /// </summary>
    /// <param name="playerControl">The player object.</param>
    /// <param name="includeImpostors">Whether impostors should be included in the search.</param>
    /// <param name="distance">The radius to search within.</param>
    /// <param name="ignoreColliders">Whether colliders should be ignored when searching.</param>
    /// <returns>The closest player if there is one, false otherwise.</returns>
    public static PlayerControl? GetClosestPlayer(this PlayerControl playerControl, bool includeImpostors, float distance, bool ignoreColliders = false)
    {
        var filteredPlayers = Helpers.GetClosestPlayers(playerControl, distance, ignoreColliders)
            .Where(playerInfo => !playerInfo.Data.Disconnected && playerInfo.PlayerId != playerControl.PlayerId && !playerInfo.Data.IsDead &&
                                 (includeImpostors || !playerInfo.Data.Role.IsImpostor));

        return filteredPlayers.FirstOrDefault();
    }
}

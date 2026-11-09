using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Networking;
using MiraAPI.Networking.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace MiraAPI.Modifiers;

/// <summary>
/// Extensions for <see cref="IBaseModifier"/>s.
/// </summary>
public static partial class ModifierExtensions
{
    /// <summary>
    /// Gets the <see cref="Dictionary{TKey, TValue}"/> to cache <see cref="ModifierComponent"/>s.
    /// </summary>
    public static Dictionary<PlayerControl, ModifierComponent> ModifierComponents { get; } = [];

    /// <summary>
    /// Remote Procedure Call to add a <see cref="IBaseModifier"/> to a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="target">The <see cref="PlayerControl"/> to add the <see cref="IBaseModifier"/> to.</param>
    /// <param name="typeId">The <see cref="IBaseModifier"/> type ID.</param>
    /// <param name="args">The arguments to initialize the <see cref="IBaseModifier"/> with.</param>
    public static void RpcAddModifier(this PlayerControl target, uint typeId, params object[] args)
    {
        _ = ModifierManager.GetModifierType(typeId) ?? throw new InvalidOperationException(
            $"Modifier with ID {typeId} is not registered.");

        Rpc<AddModifierRpc>.Instance.Send(target, new ModifierData(typeId, Guid.NewGuid(), args), true);
    }

    /// <summary>
    /// Remote Procedure Call to add a <see cref="IBaseModifier"/> to a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> to add the <see cref="IBaseModifier"/> to.</param>
    /// <param name="type">The type of the <see cref="IBaseModifier"/>.</param>
    /// <param name="args">The arguments to initialize the <see cref="IBaseModifier"/> with.</param>
    public static void RpcAddModifier(this PlayerControl player, Type type, params object[] args)
    {
        var id = ModifierManager.GetModifierTypeId(type) ?? throw new InvalidOperationException(
            $"Modifier {type.Name} is not registered.");

        player.RpcAddModifier(id, args);
    }

    /// <summary>
    /// Remote Procedure Call to add a <typeparamref name="T"/> to a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> to add the <typeparamref name="T"/> to.</param>
    /// <param name="args">The arguments to initialize the <typeparamref name="T"/> with.</param>
    /// <typeparam name="T">The <see cref="IBaseModifier"/> Type.</typeparam>
    public static void RpcAddModifier<T>(this PlayerControl player, params object[] args)
        where T : class, IBaseModifier
    {
        player.RpcAddModifier(typeof(T), args);
    }

    /// <summary>
    /// Remote Procedure Call to remove a <see cref="IBaseModifier"/> from a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="target">The <see cref="PlayerControl"/> to remove the <see cref="IBaseModifier"/> from.</param>
    /// <param name="uniqueId">The unique ID of the <see cref="IBaseModifier"/>.</param>
    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(this PlayerControl target, Guid uniqueId)
    {
        target.RemoveModifier(uniqueId);
    }

    /// <summary>
    /// Remote Procedure Call to remove a <see cref="IBaseModifier"/> from a <see cref="PlayerControl"/> by type ID.
    /// </summary>
    /// <param name="target">The <see cref="PlayerControl"/> to remove the <see cref="IBaseModifier"/> from.</param>
    /// <param name="typeId">The type ID of the <see cref="IBaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    public static void RpcRemoveModifier(
        this PlayerControl target,
        uint typeId,
        Func<IBaseModifier, bool>? predicate = null)
    {
        var modifier = target.GetModifier(typeId, predicate);
        if (modifier is null)
        {
            Error($"Player {target.PlayerId} does not have modifier with type ID {typeId}.");
            return;
        }

        target.RpcRemoveModifier(modifier.UniqueId);
    }

    /// <summary>
    /// Remote Procedure Call to remove a <see cref="IBaseModifier"/> from a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> to remove the <see cref="IBaseModifier"/> from.</param>
    /// <param name="type">The type of the <see cref="IBaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    public static void RpcRemoveModifier(
        this PlayerControl player,
        Type type,
        Func<IBaseModifier, bool>? predicate = null)
    {
        var id = ModifierManager.GetModifierTypeId(type) ?? throw new InvalidOperationException(
            $"Modifier {type.Name} is not registered.");

        player.RpcRemoveModifier(id, predicate);
    }

    /// <summary>
    /// Remote Procedure Call to remove a <typeparamref name="T"/> from a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> to remove the <typeparamref name="T"/> from.</param>
    /// <param name="predicate">Optional predicate to filter the <typeparamref name="T"/>s.</param>
    /// <typeparam name="T">The Type of the <see cref="IBaseModifier"/>.</typeparam>
    public static void RpcRemoveModifier<T>(this PlayerControl player, Func<IBaseModifier, bool>? predicate = null)
        where T : class, IBaseModifier
    {
        player.RpcRemoveModifier(typeof(T), predicate);
    }

    /// <summary>
    /// Gets the <see cref="ModifierComponent"/> for a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> object.</param>
    /// <returns>A <see cref="ModifierComponent"/> if there is one, <see langword="null"/> otherwise.</returns>
    public static ModifierComponent GetModifierComponent(this PlayerControl player)
    {
        if (ModifierComponents.TryGetValue(player, out var component))
        {
            return component;
        }

        component = player.GetComponent<ModifierComponent>();
        if (!component)
        {
            throw new InvalidOperationException("ModifierComponent is not attached to the player.");
        }

        ModifierComponents[player] = component;
        return component;
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <typeparamref name="T"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : class, IBaseModifier
    {
        return player.GetModifierComponent().HasModifier(predicate);
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific <see cref="IBaseModifier"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="IBaseModifier"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, Type type, Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().HasModifier(type, predicate);
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific <see cref="IBaseModifier"/> by type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="IBaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="IBaseModifier"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier(
        this PlayerControl player,
        uint typeId,
        Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().HasModifier(typeId, predicate);
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific modifier by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The unique ID of the <see cref="IBaseModifier"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="IBaseModifier"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, Guid uniqueId)
    {
        return player.GetModifierComponent().HasModifier(uniqueId);
    }

    /// <summary>
    /// Clears all <see cref="IBaseModifier"/>s from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="plr">The player you want to clear <see cref="IBaseModifier"/>s for.</param>
    public static void ClearModifiers(this PlayerControl plr)
    {
        foreach (var mod in plr.GetModifierComponent().ActiveBaseModifiers)
        {
            plr.RemoveModifier(mod);
        }
    }

    /// <summary>
    /// Clears all <see cref="IBaseModifier"/>s from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="plr">The player you want to clear <see cref="IBaseModifier"/>s for.</param>
    public static void RpcClearModifiers(this PlayerControl plr)
    {
        foreach (var mod in plr.GetModifierComponent().ActiveBaseModifiers)
        {
            plr.RpcRemoveModifier(mod.UniqueId);
        }
    }

    /// <summary>
    /// Tries to get a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <typeparamref name="T"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <typeparamref name="T"/> by.</param>
    /// <typeparam name="T">The Type of the <see cref="IBaseModifier"/>.</typeparam>
    /// <returns><see langword="true"/> if the <typeparamref name="T"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier<T>(this PlayerControl player, [NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null)
        where T : class, IBaseModifier
    {
        return player.GetModifierComponent().TryGetModifier(out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a <see cref="IBaseModifier"/> by its type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The modifier type.</param>
    /// <param name="modifier">The <see cref="IBaseModifier"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <see cref="IBaseModifier"/> by.</param>
    /// <returns><see langword="true"/> if the <see cref="IBaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, Type type, [NotNullWhen(true)] out IBaseModifier? modifier, Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryGetModifier(type, out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a <see cref="IBaseModifier"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="id">The <see cref="IBaseModifier"/> type ID.</param>
    /// <param name="modifier">The <see cref="IBaseModifier"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <see cref="IBaseModifier"/> by.</param>
    /// <returns><see langword="true"/> if the <see cref="IBaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, uint id, [NotNullWhen(true)] out IBaseModifier? modifier, Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryGetModifier(id, out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a <see cref="IBaseModifier"/> by its unique ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifierGuid">The <see cref="IBaseModifier"/> unique ID.</param>
    /// <param name="modifier">The <see cref="IBaseModifier"/> or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="IBaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, Guid modifierGuid, [NotNullWhen(true)] out IBaseModifier? modifier)
    {
        return player.GetModifierComponent().TryGetModifier(modifierGuid, out modifier);
    }

    /// <summary>
    /// Gets a specific <typeparamref name="T"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <typeparamref name="T"/>s.</param>
    /// <returns>The <typeparamref name="T"/> if found, <see langword="null"/> otherwise.</returns>
    public static T? GetModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : class, IBaseModifier
    {
        return player.GetModifierComponent().GetModifier(predicate);
    }

    /// <summary>
    /// Gets a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns>The <see cref="IBaseModifier"/> if found, <see langword="null"/> otherwise.</returns>
    public static IBaseModifier? GetModifier(
        this PlayerControl player,
        Type type,
        Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifier(type, predicate);
    }

    /// <summary>
    /// Gets a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="IBaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns>The <see cref="IBaseModifier"/> if found, <see langword="null"/> otherwise.</returns>
    public static IBaseModifier? GetModifier(
        this PlayerControl player,
        uint typeId,
        Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifier(typeId, predicate);
    }

    /// <summary>
    /// Gets a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The GUID of the <see cref="IBaseModifier"/>.</param>
    /// <returns>The <see cref="IBaseModifier"/> if found, <see langword="null"/> otherwise.</returns>
    public static IBaseModifier? GetModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        return player.GetModifierComponent().GetModifier(uniqueId);
    }

    /// <summary>
    /// Gets all <typeparamref name="T"/>s from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <typeparamref name="T"/>s.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <typeparamref name="T"/>s.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s.</returns>
    public static IEnumerable<T> GetModifiers<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : class, IBaseModifier
    {
        return player.GetModifierComponent().GetModifiers(predicate);
    }

    /// <summary>
    /// Gets all <see cref="IBaseModifier"/>s of a specific type from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifiers.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IBaseModifier"/>s.</returns>
    public static IEnumerable<IBaseModifier> GetModifiers(
        this PlayerControl player,
        Type type,
        Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifiers(type, predicate);
    }

    /// <summary>
    /// Gets all <see cref="IBaseModifier"/>s of a specific type from the <see cref="PlayerControl"/> by type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="IBaseModifier"/>s.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IBaseModifier"/>s.</returns>
    public static IEnumerable<IBaseModifier> GetModifiers(
        this PlayerControl player,
        uint typeId,
        Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifiers(typeId, predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <see cref="IBaseModifier"/> to remove.</param>
    public static void RemoveModifier(this PlayerControl player, IBaseModifier modifier)
    {
        player.GetModifierComponent().RemoveModifier(modifier);
    }

    /// <summary>
    /// Removes a specific <typeparamref name="T"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    public static void RemoveModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : class, IBaseModifier
    {
        player.GetModifierComponent().RemoveModifier(predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    public static void RemoveModifier(this PlayerControl player, Type type, Func<IBaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent().RemoveModifier(type, predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="IBaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    public static void RemoveModifier(
        this PlayerControl player,
        uint typeId,
        Func<IBaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent().RemoveModifier(typeId, predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The GUID of the <see cref="IBaseModifier"/>.</param>
    public static void RemoveModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        player.GetModifierComponent().RemoveModifier(uniqueId);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <see cref="IBaseModifier"/> to remove.</param>
    /// <returns><see langword="false"/> if the <see cref="IBaseModifier"/> is not active on this <see cref="PlayerControl"/>, else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(this PlayerControl player, IBaseModifier modifier)
    {
        return player.GetModifierComponent().TryRemoveModifier(modifier);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns><see langword="false"/> if the <see cref="IBaseModifier"/> is not active on this <see cref="PlayerControl"/>, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : class, IBaseModifier
    {
        return player.GetModifierComponent().TryRemoveModifier(predicate);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the <see cref="IBaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns><see langword="false"/> if the <see cref="IBaseModifier"/> is not active on this <see cref="PlayerControl"/>, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(this PlayerControl player, Type type, Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryRemoveModifier(type, predicate);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="IBaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="IBaseModifier"/>s.</param>
    /// <returns><see langword="false"/> if the <see cref="IBaseModifier"/> is not active on this <see cref="PlayerControl"/>, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(
        this PlayerControl player,
        uint typeId,
        Func<IBaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryRemoveModifier(typeId, predicate);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="IBaseModifier"/> from the <see cref="PlayerControl"/> by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The GUID of the <see cref="IBaseModifier"/>.</param>
    /// <returns><see langword="false"/> if the <see cref="IBaseModifier"/> is not active on this <see cref="PlayerControl"/>, else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        return player.GetModifierComponent().TryRemoveModifier(uniqueId);
    }

    /// <summary>
    /// Adds a specific <typeparamref name="T"/> to the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IBaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="args">The arguments to initialize the <typeparamref name="T"/> with.</param>
    /// <returns>The added <typeparamref name="T"/>.</returns>
    public static T? AddModifier<T>(this PlayerControl player, params object[] args)
        where T : class, IBaseModifier
    {
        return player.GetModifierComponent().AddModifier<T>(args);
    }

    /// <summary>
    /// Adds a specific <see cref="IBaseModifier"/> to the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <see cref="IBaseModifier"/> to add.</param>
    /// <returns>The added <see cref="IBaseModifier"/>.</returns>
    public static IBaseModifier? AddModifier(this PlayerControl player, IBaseModifier modifier)
    {
        return player.GetModifierComponent().AddModifier(modifier);
    }

    /// <summary>
    /// Adds a specific <see cref="IBaseModifier"/> to the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="args">The arguments to initialize the <see cref="IBaseModifier"/> with.</param>
    /// <returns>The added <see cref="IBaseModifier"/>.</returns>
    public static IBaseModifier? AddModifier(this PlayerControl player, Type type, params object[] args)
    {
        return player.GetModifierComponent().AddModifier(type, args);
    }

    /// <summary>
    /// Adds a specific <see cref="IBaseModifier"/> to the <see cref="PlayerControl"/> by type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="IBaseModifier"/>.</param>
    /// <param name="args">The arguments to initialize the <see cref="IBaseModifier"/> with.</param>
    /// <returns>The added <see cref="IBaseModifier"/>.</returns>
    public static IBaseModifier? AddModifier(this PlayerControl player, uint typeId, params object[] args)
    {
        return player.GetModifierComponent().AddModifier(typeId, args);
    }
}

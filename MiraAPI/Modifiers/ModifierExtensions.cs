using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Networking;
using MiraAPI.Networking.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace MiraAPI.Modifiers;

/// <summary>
/// Extensions for <see cref="BaseModifier"/>s.
/// </summary>
public static class ModifierExtensions
{
    /// <summary>
    /// Gets the <see cref="Dictionary{TKey, TValue}"/> to cache <see cref="ModifierComponent"/>s.
    /// </summary>
    public static Dictionary<PlayerControl, ModifierComponent> ModifierComponents { get; } = [];

    /// <summary>
    /// Remote Procedure Call to add a <see cref="BaseModifier"/> to a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="target">The <see cref="PlayerControl"/> to add the <see cref="BaseModifier"/> to.</param>
    /// <param name="typeId">The <see cref="BaseModifier"/> type ID.</param>
    /// <param name="args">The arguments to initialize the <see cref="BaseModifier"/> with.</param>
    public static void RpcAddModifier(this PlayerControl target, uint typeId, params object[] args)
    {
        _ = ModifierManager.GetModifierType(typeId) ?? throw new InvalidOperationException(
            $"Modifier with ID {typeId} is not registered.");

        Rpc<AddModifierRpc>.Instance.Send(target, new ModifierData(typeId, Guid.NewGuid(), args), true);
    }

    /// <summary>
    /// Remote Procedure Call to add a <see cref="BaseModifier"/> to a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> to add the <see cref="BaseModifier"/> to.</param>
    /// <param name="type">The type of the <see cref="BaseModifier"/>.</param>
    /// <param name="args">The arguments to initialize the <see cref="BaseModifier"/> with.</param>
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
    /// <typeparam name="T">The <see cref="BaseModifier"/> Type.</typeparam>
    public static void RpcAddModifier<T>(this PlayerControl player, params object[] args) where T : BaseModifier
    {
        player.RpcAddModifier(typeof(T), args);
    }

    /// <summary>
    /// Remote Procedure Call to remove a <see cref="BaseModifier"/> from a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="target">The <see cref="PlayerControl"/> to remove the <see cref="BaseModifier"/> from.</param>
    /// <param name="uniqueId">The unique ID of the <see cref="BaseModifier"/>.</param>
    [MethodRpc((uint)MiraRpc.RemoveModifier)]
    public static void RpcRemoveModifier(this PlayerControl target, Guid uniqueId)
    {
        target.RemoveModifier(uniqueId);
    }

    /// <summary>
    /// Remote Procedure Call to remove a <see cref="BaseModifier"/> from a <see cref="PlayerControl"/> by type ID.
    /// </summary>
    /// <param name="target">The <see cref="PlayerControl"/> to remove the <see cref="BaseModifier"/> from.</param>
    /// <param name="typeId">The type ID of the <see cref="BaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    public static void RpcRemoveModifier(
        this PlayerControl target,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
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
    /// Remote Procedure Call to remove a <see cref="BaseModifier"/> from a <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> to remove the <see cref="BaseModifier"/> from.</param>
    /// <param name="type">The type of the <see cref="BaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    public static void RpcRemoveModifier(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
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
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>.</typeparam>
    public static void RpcRemoveModifier<T>(this PlayerControl player, Func<BaseModifier, bool>? predicate = null)
        where T : BaseModifier
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
    /// <typeparam name="T">The type of the <see cref="BaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <typeparamref name="T"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent().HasModifier(predicate);
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific <see cref="BaseModifier"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="BaseModifier"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, Type type, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().HasModifier(type, predicate);
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific <see cref="BaseModifier"/> by type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="BaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="BaseModifier"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().HasModifier(typeId, predicate);
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific modifier by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The unique ID of the <see cref="BaseModifier"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="BaseModifier"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifier(this PlayerControl player, Guid uniqueId)
    {
        return player.GetModifierComponent().HasModifier(uniqueId);
    }

    /// <summary>
    /// Checks if the <see cref="PlayerControl"/> has a specific <see cref="BaseModifier"/>, if the type is an interface.
    /// </summary>
    /// <typeparam name="T">The type of the interface of the <see cref="BaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns><see langword="true"/> if the <see cref="PlayerControl"/> has the <see cref="BaseModifier"/> of type <typeparamref name="T"/>, <see langword="false"/> otherwise.</returns>
    public static bool HasModifierOfType<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : class
    {
        return player.GetModifierComponent().HasModifierOfType(predicate);
    }

    /// <summary>
    /// Clears all <see cref="BaseModifier"/>s from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="plr">The player you want to clear <see cref="BaseModifier"/>s for.</param>
    public static void ClearModifiers(this PlayerControl plr)
    {
        foreach (var mod in plr.GetModifierComponent().ActiveModifiers)
        {
            plr.RemoveModifier(mod);
        }
    }

    /// <summary>
    /// Clears all <see cref="BaseModifier"/>s from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="plr">The player you want to clear <see cref="BaseModifier"/>s for.</param>
    public static void RpcClearModifiers(this PlayerControl plr)
    {
        foreach (var mod in plr.GetModifierComponent().ActiveModifiers)
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
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns><see langword="true"/> if the <typeparamref name="T"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier<T>(this PlayerControl player, [NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent().TryGetModifier(out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The modifier type.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, Type type, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryGetModifier(type, out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="id">The <see cref="BaseModifier"/> type ID.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, uint id, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryGetModifier(id, out modifier, predicate);
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its unique ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifierGuid">The <see cref="BaseModifier"/> unique ID.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifier(this PlayerControl player, Guid modifierGuid, [NotNullWhen(true)] out BaseModifier? modifier)
    {
        return player.GetModifierComponent().TryGetModifier(modifierGuid, out modifier);
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its type, if the type is an interface.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> of type <typeparamref name="T"/> or null.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> of type <typeparamref name="T"/> by.</param>
    /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> of type <typeparamref name="T"/> was found, <see langword="false"/> otherwise.</returns>
    public static bool TryGetModifierOfType<T>(this PlayerControl player, [NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null) where T : class
    {
        return player.GetModifierComponent().TryGetModifierOfType(out modifier, predicate);
    }

    /// <summary>
    /// Gets a specific <typeparamref name="T"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="BaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <typeparamref name="T"/>s.</param>
    /// <returns>The <typeparamref name="T"/> if found, <see langword="null"/> otherwise.</returns>
    public static T? GetModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return player.GetModifierComponent().GetModifier(predicate);
    }

    /// <summary>
    /// Gets a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns>The <see cref="BaseModifier"/> if found, <see langword="null"/> otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifier(type, predicate);
    }

    /// <summary>
    /// Gets a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="BaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns>The <see cref="BaseModifier"/> if found, <see langword="null"/> otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifier(typeId, predicate);
    }

    /// <summary>
    /// Gets a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The GUID of the <see cref="BaseModifier"/>.</param>
    /// <returns>The <see cref="BaseModifier"/> if found, <see langword="null"/> otherwise.</returns>
    public static BaseModifier? GetModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        return player.GetModifierComponent().GetModifier(uniqueId);
    }

    /// <summary>
    /// Gets a specific <see cref="BaseModifier"/> of type <typeparamref name="T"/> from the <see cref="PlayerControl"/>, if the type is an interface.
    /// </summary>
    /// <typeparam name="T">The type of the interface of the modifier.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns>The <see cref="BaseModifier"/> of type <typeparamref name="T"/> if found, <see langword="null"/> otherwise.</returns>
    public static T? GetModifierOfType<T>(this PlayerControl player, Func<T, bool>? predicate = null) where T : class
    {
        return player.GetModifierComponent().GetModifierOfType(predicate);
    }

    /// <summary>
    /// Gets all <typeparamref name="T"/>s from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <typeparamref name="T"/>s.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <typeparamref name="T"/>s.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s.</returns>
    public static IEnumerable<T> GetModifiers<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : BaseModifier
    {
        return player.GetModifierComponent().GetModifiers(predicate);
    }

    /// <summary>
    /// Gets all <see cref="BaseModifier"/>s of a specific type from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifiers.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s.</returns>
    public static IEnumerable<BaseModifier> GetModifiers(
        this PlayerControl player,
        Type type,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifiers(type, predicate);
    }

    /// <summary>
    /// Gets all <see cref="BaseModifier"/>s of a specific type from the <see cref="PlayerControl"/> by type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="BaseModifier"/>s.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s.</returns>
    public static IEnumerable<BaseModifier> GetModifiers(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().GetModifiers(typeId, predicate);
    }

    /// <summary>
    /// Gets all <see cref="BaseModifier"/>s of a specific type from the <see cref="PlayerControl"/>, if the type is an interface.
    /// </summary>
    /// <typeparam name="T">The type of the interface of the <see cref="BaseModifier"/>s.</typeparam>
    /// <param name="player">The PlayerControl instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetModifiersOfType<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : class
    {
        return player.GetModifierComponent().GetModifiersOfType(predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> to remove.</param>
    public static void RemoveModifier(this PlayerControl player, BaseModifier modifier)
    {
        player.GetModifierComponent().RemoveModifier(modifier);
    }

    /// <summary>
    /// Removes a specific <typeparamref name="T"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="BaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    public static void RemoveModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : BaseModifier
    {
        player.GetModifierComponent().RemoveModifier(predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    public static void RemoveModifier(this PlayerControl player, Type type, Func<BaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent().RemoveModifier(type, predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="BaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    public static void RemoveModifier(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        player.GetModifierComponent().RemoveModifier(typeId, predicate);
    }

    /// <summary>
    /// Removes a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The GUID of the <see cref="BaseModifier"/>.</param>
    public static void RemoveModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        player.GetModifierComponent().RemoveModifier(uniqueId);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> to remove.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this <see cref="PlayerControl"/>, else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(this PlayerControl player, BaseModifier modifier)
    {
        return player.GetModifierComponent().TryRemoveModifier(modifier);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="BaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this <see cref="PlayerControl"/>, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier<T>(this PlayerControl player, Func<T, bool>? predicate = null)
        where T : BaseModifier
    {
        return player.GetModifierComponent().TryRemoveModifier(predicate);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the <see cref="BaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this <see cref="PlayerControl"/>, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(this PlayerControl player, Type type, Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryRemoveModifier(type, predicate);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by its type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="BaseModifier"/>.</param>
    /// <param name="predicate">Optional predicate to filter the <see cref="BaseModifier"/>s.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this <see cref="PlayerControl"/>, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(
        this PlayerControl player,
        uint typeId,
        Func<BaseModifier, bool>? predicate = null)
    {
        return player.GetModifierComponent().TryRemoveModifier(typeId, predicate);
    }

    /// <summary>
    /// Tries to remove a specific <see cref="BaseModifier"/> from the <see cref="PlayerControl"/> by its GUID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="uniqueId">The GUID of the <see cref="BaseModifier"/>.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this <see cref="PlayerControl"/>, else <see langword="true"/>.</returns>
    public static bool TryRemoveModifier(
        this PlayerControl player,
        Guid uniqueId)
    {
        return player.GetModifierComponent().TryRemoveModifier(uniqueId);
    }

    /// <summary>
    /// Adds a specific <typeparamref name="T"/> to the <see cref="PlayerControl"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="BaseModifier"/>.</typeparam>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="args">The arguments to initialize the <typeparamref name="T"/> with.</param>
    /// <returns>The added <typeparamref name="T"/>.</returns>
    public static T? AddModifier<T>(this PlayerControl player, params object[] args) where T : BaseModifier
    {
        return player.GetModifierComponent().AddModifier<T>(args);
    }

    /// <summary>
    /// Adds a specific <see cref="BaseModifier"/> to the <see cref="PlayerControl"/>.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> to add.</param>
    /// <returns>The added <see cref="BaseModifier"/>.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, BaseModifier modifier)
    {
        return player.GetModifierComponent().AddModifier(modifier);
    }

    /// <summary>
    /// Adds a specific <see cref="BaseModifier"/> to the <see cref="PlayerControl"/> by type.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="type">The type of the modifier.</param>
    /// <param name="args">The arguments to initialize the <see cref="BaseModifier"/> with.</param>
    /// <returns>The added <see cref="BaseModifier"/>.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, Type type, params object[] args)
    {
        return player.GetModifierComponent().AddModifier(type, args);
    }

    /// <summary>
    /// Adds a specific <see cref="BaseModifier"/> to the <see cref="PlayerControl"/> by type ID.
    /// </summary>
    /// <param name="player">The <see cref="PlayerControl"/> instance.</param>
    /// <param name="typeId">The type ID of the <see cref="BaseModifier"/>.</param>
    /// <param name="args">The arguments to initialize the <see cref="BaseModifier"/> with.</param>
    /// <returns>The added <see cref="BaseModifier"/>.</returns>
    public static BaseModifier? AddModifier(this PlayerControl player, uint typeId, params object[] args)
    {
        return player.GetModifierComponent().AddModifier(typeId, args);
    }
}

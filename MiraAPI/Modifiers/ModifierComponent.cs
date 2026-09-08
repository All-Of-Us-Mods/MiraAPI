using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.LocalSettings;
using MiraAPI.Modifiers.ModifierDisplay;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Roles;
using MiraAPI.Translation;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.Modifiers;

/// <summary>
/// The component for handling <see cref="BaseModifier"/>s.
/// </summary>
[RegisterInIl2Cpp]
public class ModifierComponent(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the active <see cref="BaseModifier"/>s on the player.
    /// </summary>
    [HideFromIl2Cpp]
    public ImmutableList<BaseModifier> ActiveModifiers { get; private set; } = ImmutableList<BaseModifier>.Empty;

    private ModifierDisplayComponent? ModifierDisplay { get; set; }

    [HideFromIl2Cpp]
    private List<BaseModifier> Modifiers { get; set; } = [];

    private PlayerControl _player = null!;

    private readonly List<BaseModifier> _toRemove = [];

    private readonly List<BaseModifier> _toAdd = [];

    internal void ClearModifiers()
    {
        _toRemove.AddRange(Modifiers);
    }

    private void Awake()
    {
        _player = GetComponent<PlayerControl>();
        if (_player == null)
        {
            Error("ModifierComponent could not find PlayerControl on the same GameObject.");
            Destroy(this);
            return;
        }

        ModifierExtensions.ModifierComponents.Add(_player, this);

        Modifiers = [];

        if (!_player.AmOwner)
        {
            return;
        }

        ModifierDisplay = ModifierDisplayComponent.CreateDisplay();
        ModifierDisplay?.UpdateModifiersList(Modifiers);
    }

    private void FixedUpdate()
    {
        var removed = _toRemove.Count > 0;
        foreach (var modifier in _toRemove.ToArray())
        {
            _toRemove.Remove(modifier);
            try
            {
                modifier.OnDeactivate();
            }
            catch (Exception e)
            {
                Error($"Error while deactivating modifier {modifier.ModifierName}: {e.ToString()}");
            }

            Modifiers.Remove(modifier);
        }

        var added = _toAdd.Count > 0;
        foreach (var modifier in _toAdd.ToArray())
        {
            _toAdd.Remove(modifier);
            Modifiers.Add(modifier);
            modifier.Initialized = true;
            try
            {
                modifier.OnActivate();
            }
            catch (Exception e)
            {
                Error($"Error while activating modifier {modifier.ModifierName}: {e.ToString()}");
            }

            if (modifier is TimedModifier { AutoStart: true } timer)
            {
                timer.StartTimer();
            }
        }

        if (removed || added)
        {
            if (_player.AmOwner && HudManager.InstanceExists)
            {
                ModifierDisplay?.UpdateModifiersList(Modifiers);
            }

            ActiveModifiers = Modifiers.ToImmutableList();
        }

        foreach (var modifier in ActiveModifiers)
        {
            try
            {
                modifier.FixedUpdate();
            }
            catch (Exception e)
            {
                Error($"Error while (fixed) updating modifier {modifier.ModifierName}: {e.ToString()}");
            }
        }

        if (_player.AmOwner && ModifierDisplay && HudManager.InstanceExists)
        {
            var taskPanelOpen = HudManager.Instance.TaskPanel.open;
            var roleTabOpen = HudManagerPatches.RoleTab != null && HudManagerPatches.RoleTab.open;
            var leftSideHud = LocalSettingsTabSingleton<MiraApiSettings>.Instance.ModifiersHudLeftSide.Value;

            if (leftSideHud && (taskPanelOpen || roleTabOpen))
            {
                ModifierDisplay!.gameObject.SetActive(false);
            }
            else
            {
                ModifierDisplay!.gameObject.SetActive(MeetingHud.Instance || HudManager.Instance.TaskPanel.isActiveAndEnabled);
            }
        }
    }

    private void Update()
    {
        foreach (var modifier in ActiveModifiers)
        {
            try
            {
                modifier.Update();
            }
            catch (Exception e)
            {
                Error($"Error while updating modifier {modifier.ModifierName}: {e.ToString()}");
            }
        }
    }

    private void OnDestroy()
    {
        // Remove this component from the global list when destroyed
        ModifierExtensions.ModifierComponents.Remove(_player);
    }

    /// <summary>
    /// Gets a collection of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="predicate">The predicate to check the <typeparamref name="T"/> by.</param>
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>e.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/>s.</returns>
    [HideFromIl2Cpp]
    public IEnumerable<T> GetModifiers<T>(Func<T, bool>? predicate=null) where T : BaseModifier
    {
        return ActiveModifiers.OfType<T>().Where(x => predicate == null || predicate(x));
    }

    /// <summary>
    /// Gets a collection of <see cref="BaseModifier"/>s by type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s.</returns>
    [HideFromIl2Cpp]
    public IEnumerable<BaseModifier> GetModifiers(Type type, Func<BaseModifier, bool>? predicate=null)
    {
        return ActiveModifiers.Where(x => x.GetType() == type && (predicate == null || predicate(x)));
    }

    /// <summary>
    /// Gets a collection of <see cref="BaseModifier"/>s by type ID.
    /// </summary>
    /// <param name="id">The <see cref="BaseModifier"/>'s type ID.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s.</returns>
    [HideFromIl2Cpp]
    public IEnumerable<BaseModifier> GetModifiers(uint id, Func<BaseModifier, bool>? predicate=null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return GetModifiers(type, predicate);
    }

    /// <summary>
    /// Gets a collection of <see cref="BaseModifier"/>s by type, if the type is an interface.
    /// </summary>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BaseModifier"/>s of type <typeparamref name="T"/>.</returns>
    [HideFromIl2Cpp]
    public IEnumerable<T> GetModifiersOfType<T>(Func<T, bool>? predicate = null) where T : class
    {
        return ActiveModifiers.OfType<T>().Where(x => predicate == null || predicate(x));
    }

    /// <summary>
    /// Tries to get a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="modifier">The <typeparamref name="T"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <typeparamref name="T"/> by.</param>
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns><see langword="true"/> if the <typeparamref name="T"/> was found, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier<T>([NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null) where T : BaseModifier
    {
        modifier = GetModifier(predicate);
        return modifier != null;
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier(Type type, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        modifier = GetModifier(type, predicate);
        return modifier != null;
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its type ID.
    /// </summary>
    /// <param name="id">The <see cref="BaseModifier"/>'s type ID.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> or <see langword="null"/>.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier(uint id, [NotNullWhen(true)] out BaseModifier? modifier, Func<BaseModifier, bool>? predicate = null)
    {
        modifier = GetModifier(id, predicate);
        return modifier != null;
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its unique ID.
    /// </summary>
    /// <param name="modifierGuid">The <see cref="BaseModifier"/>'s unique ID.</param>
    /// <param name="modifier">The <see cref="BaseModifier"/> or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> was found, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifier(Guid modifierGuid, [NotNullWhen(true)] out BaseModifier? modifier)
    {
        modifier = GetModifier(modifierGuid);
        return modifier != null;
    }

    /// <summary>
    /// Tries to get a <see cref="BaseModifier"/> by its type, if the type is an interface.
    /// </summary>
    /// <param name="modifier">The <see cref="BaseModifier"/> of type <typeparamref name="T"/> or null.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> of type <typeparamref name="T"/> was found, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool TryGetModifierOfType<T>([NotNullWhen(true)] out T? modifier, Func<T, bool>? predicate = null) where T : class
    {
        modifier = GetModifierOfType(predicate);
        return modifier != null;
    }

    /// <summary>
    /// Gets a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="predicate">The predicate to check the <typeparamref name="T"/> by.</param>
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns>The <typeparamref name="T"/> if it is found, <see langword="null"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public T? GetModifier<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return GetModifiers(predicate).FirstOrDefault();
    }

    /// <summary>
    /// Gets a <see cref="BaseModifier"/> by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns>The <see cref="BaseModifier"/> if it is found, <see langword="null"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? GetModifier(Type type, Func<BaseModifier, bool>? predicate = null)
    {
        return GetModifiers(type).FirstOrDefault(predicate ?? (_ => true));
    }

    /// <summary>
    /// Gets a <see cref="BaseModifier"/> by its type ID.
    /// </summary>
    /// <param name="id">The <see cref="BaseModifier"/> ID.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns>The <see cref="BaseModifier"/> if it is found, <see langword="null"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? GetModifier(uint id, Func<BaseModifier, bool>? predicate = null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return GetModifier(type, predicate);
    }

    /// <summary>
    /// Gets a <see cref="BaseModifier"/> by unique ID.
    /// </summary>
    /// <param name="modifierGuid">The <see cref="BaseModifier"/>'s unique ID.</param>
    /// <returns>The <see cref="BaseModifier"/> if it is found, or <see langword="null"/>.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? GetModifier(Guid modifierGuid)
    {
        return ActiveModifiers.Find(x => x.UniqueId == modifierGuid);
    }

    /// <summary>
    /// Gets a <see cref="BaseModifier"/> by its type, if the type is an interface.
    /// </summary>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <typeparam name="T">The Type of the interface of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns>The <see cref="BaseModifier"/> of type <typeparamref name="T"/> if it is found, <see langword="null"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public T? GetModifierOfType<T>(Func<T, bool>? predicate = null) where T : class
    {
        return GetModifiersOfType(predicate).FirstOrDefault();
    }

    /// <summary>
    /// Removes a <typeparamref name="T"/> from the player.
    /// </summary>
    /// <typeparam name="T">The <see cref="BaseModifier"/> type.</typeparam>
    /// <param name="predicate">The predicate to check the <typeparamref name="T"/> by.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        RemoveModifier(typeof(T), x => predicate == null || predicate((T)x));
    }

    /// <summary>
    /// Removes a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(Type type, Func<BaseModifier, bool>? predicate = null)
    {
        var modifiers = ActiveModifiers.Where(x => x.GetType() == type && (predicate == null || predicate(x))).ToList();
        if (modifiers.Count > 1)
        {
            throw new InvalidOperationException($"Cannot remove modifier {type.Name} because there are multiple instances of that modifier.");
        }

        var modifier = modifiers.FirstOrDefault();
        if (modifier is null)
        {
            Error($"Cannot remove modifier {type.Name} because it is not active.");
            return;
        }

        RemoveModifier(modifier);
    }

    /// <summary>
    /// Removes a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="modifier">The <see cref="BaseModifier"/> object.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(BaseModifier modifier)
    {
        if (!ActiveModifiers.Contains(modifier))
        {
            Error($"Cannot remove modifier {modifier.ModifierName} because it is not active on this player.");
            return;
        }

        _toRemove.Add(modifier);
    }

    /// <summary>
    /// Removes a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="typeId">The <see cref="BaseModifier"/>'s type ID.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(uint typeId, Func<BaseModifier, bool>? predicate = null)
    {
        var type = ModifierManager.GetModifierType(typeId) ?? throw new InvalidOperationException(
            $"Cannot remove modifier with id {typeId} because it is not registered.");
        RemoveModifier(type, predicate);
    }

    /// <summary>
    /// Removes a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="uniqueId">The <see cref="BaseModifier"/>'s unique ID.</param>
    [HideFromIl2Cpp]
    public void RemoveModifier(Guid uniqueId)
    {
        var modifier = GetModifier(uniqueId);
        if (modifier == null)
        {
            Error($"Cannot remove modifier with unique id {uniqueId} because it is not active.");
            return;
        }

        RemoveModifier(modifier);
    }

    /// <summary>
    /// Tries to remove a <typeparamref name="T"/> from the player.
    /// </summary>
    /// <typeparam name="T">The <see cref="BaseModifier"/> type.</typeparam>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns><see langword="false"/> if the <typeparamref name="T"/> is not active on this player, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    [HideFromIl2Cpp]
    public bool TryRemoveModifier<T>(Func<T, bool>? predicate = null) where T : BaseModifier
    {
        return TryGetModifier(out var modifier, predicate) &&
               TryRemoveModifier(modifier);
    }

    /// <summary>
    /// Tries to remove a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="type">The <see cref="BaseModifier"/> type.</param>
    /// <param name="predicate">The predicate to check the modifier by.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this player, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    [HideFromIl2Cpp]
    public bool TryRemoveModifier(Type type, Func<BaseModifier, bool>? predicate = null)
    {
        return TryGetModifier(type, out var modifier, predicate) &&
               TryRemoveModifier(modifier);
    }

    /// <summary>
    /// Tries to remove a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="modifier">The <see cref="BaseModifier"/> object.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this player, else <see langword="true"/>.</returns>
    [HideFromIl2Cpp]
    public bool TryRemoveModifier(BaseModifier modifier)
    {
        if (!ActiveModifiers.Contains(modifier))
        {
            return false;
        }

        _toRemove.Add(modifier);
        return true;
    }

    /// <summary>
    /// Tries to remove a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="typeId">The <see cref="BaseModifier"/>'s type ID.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/> by.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this player, or there are multiple instances;
    ///     else <see langword="true"/>.</returns>
    [HideFromIl2Cpp]
    public bool TryRemoveModifier(uint typeId, Func<BaseModifier, bool>? predicate = null)
    {
        return TryGetModifier(typeId, out var modifier, predicate) &&
               TryRemoveModifier(modifier);
    }

    /// <summary>
    /// Tries to remove a <see cref="BaseModifier"/> from the player.
    /// </summary>
    /// <param name="uniqueId">The <see cref="BaseModifier"/>'s unique ID.</param>
    /// <returns><see langword="false"/> if the <see cref="BaseModifier"/> is not active on this player, else <see langword="true"/>.</returns>
    [HideFromIl2Cpp]
    public bool TryRemoveModifier(Guid uniqueId)
    {
        return TryGetModifier(uniqueId, out var modifier) &&
               TryRemoveModifier(modifier);
    }

    /// <summary>
    /// Adds a <typeparamref name="T"/> to the player.
    /// </summary>
    /// <param name="args">The arguments to initialize the <typeparamref name="T"/> with.</param>
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns>The new <typeparamref name="T"/>.</returns>
    [HideFromIl2Cpp]
    public T? AddModifier<T>(params object[] args) where T : BaseModifier
    {
        return AddModifier(typeof(T), args) as T;
    }

    /// <summary>
    /// Adds a <see cref="BaseModifier"/> to the player.
    /// </summary>
    /// <param name="modifier">The <see cref="BaseModifier"/> to add.</param>
    /// <returns>The <see cref="BaseModifier"/> that was added.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(BaseModifier modifier)
    {
        // TODO: Make a proper synchronization system.
        if (LobbyBehaviour.Instance)
        {
            Warning($"Modifiers added in the lobby won't sync to new players!");
        }

        var id = modifier.TypeId;
        if (modifier.Unique && ActiveModifiers.Find(x => x.TypeId == id) != null)
        {
            Error($"Player already has modifier with id {id}!");
            return null;
        }

        if (ActiveModifiers.Contains(modifier))
        {
            Error($"Player already has this modifier!");
            return null;
        }

        _toAdd.Add(modifier);
        modifier.Player = _player;
        modifier.ModifierComponent = this;
        if (modifier.UniqueId == Guid.Empty)
        {
            modifier.UniqueId = Guid.NewGuid();
        }
        return modifier;
    }

    /// <summary>
    /// Adds a <see cref="BaseModifier"/> to the player.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="args">The arguments to initialize the <see cref="BaseModifier"/> with.</param>
    /// <returns>The <see cref="BaseModifier"/> that was added.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(Type type, params object[] args)
    {
        BaseModifier? modifier;
        if (args.Length > 0)
        {
            modifier = ModifierFactory.CreateInstance(type, args);
        }
        else
        {
            modifier = Activator.CreateInstance(type) as BaseModifier;
            if (modifier == null)
            {
                throw new InvalidOperationException($"Cannot add modifier {type.Name} because it is not a valid modifier.");
            }
        }

        return AddModifier(modifier);
    }

    /// <summary>
    /// Adds a <see cref="BaseModifier"/> to the player.
    /// </summary>
    /// <param name="id">The ID of the <see cref="BaseModifier"/>.</param>
    /// <param name="args">The arguments to initialize the <see cref="BaseModifier"/> with.</param>
    /// <returns>The <see cref="BaseModifier"/> if it was created, or <see langword="null"/> if it failed.</returns>
    [HideFromIl2Cpp]
    public BaseModifier? AddModifier(uint id, params object[] args)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot add modifier with id {id} because it is not registered.");

        return AddModifier(type, args);
    }

    /// <summary>
    /// Checks if a player has an active <typeparamref name="T"/>.
    /// </summary>
    /// <param name="predicate">The predicate to check the <typeparamref name="T"/>.</param>
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns><see langword="true"/> if the <typeparamref name="T"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier<T>(Func<T, bool>? predicate=null) where T : BaseModifier
    {
        return ActiveModifiers.Exists(x => x is T modifier && (predicate == null || predicate(modifier)));
    }

    /// <summary>
    /// Checks if a player has an active <see cref="BaseModifier"/> by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Type type, Func<BaseModifier, bool>? predicate=null)
    {
        return ActiveModifiers.Exists(x => x.GetType() == type && (predicate == null || predicate(x)));
    }

    /// <summary>
    /// Checks if a player has an active <see cref="BaseModifier"/> by its type ID.
    /// </summary>
    /// <param name="id">The <see cref="BaseModifier"/>'s type ID.</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(uint id, Func<BaseModifier, bool>? predicate=null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return HasModifier(type, predicate);
    }

    /// <summary>
    /// Checks if a player has an active <see cref="BaseModifier"/> by its unique ID.
    /// </summary>
    /// <param name="id">The <see cref="BaseModifier"/>'s guid.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Guid id)
    {
        return ActiveModifiers.Exists(x => x.UniqueId == id);
    }

    /// <summary>
    /// Checks if a player has an active <typeparamref name="T"/>.
    /// </summary>
    /// <param name="checkInactive">Whether to check inactive <typeparamref name="T"/>s (those pending to be added).</param>
    /// <param name="predicate">The predicate to check the <typeparamref name="T"/>.</param>
    /// <typeparam name="T">The Type of the <see cref="BaseModifier"/>.</typeparam>
    /// <returns><see langword="true"/> if the <typeparamref name="T"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier<T>(bool checkInactive, Func<T, bool>? predicate=null) where T : BaseModifier
    {
        return ActiveModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
        bool MatchExpr(BaseModifier bm) => bm is T modifier && (predicate == null || predicate(modifier));
    }

    /// <summary>
    /// Checks if a player has an active <see cref="BaseModifier"/> by its type.
    /// </summary>
    /// <param name="type">The modifier type.</param>
    /// <param name="checkInactive">Whether to check inactive <see cref="BaseModifier"/>s (those pending to be added).</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Type type, bool checkInactive, Func<BaseModifier, bool>? predicate=null)
    {
        return ActiveModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
        bool MatchExpr(BaseModifier bm) => bm.GetType() == type && (predicate == null || predicate(bm));
    }

    /// <summary>
    /// Checks if a player has an active <see cref="BaseModifier"/> by its type ID.
    /// </summary>
    /// <param name="id">The <see cref="BaseModifier"/>'s type ID.</param>
    /// <param name="checkInactive">Whether to check inactive <see cref="BaseModifier"/>s (those pending to be added).</param>
    /// <param name="predicate">The predicate to check the <see cref="BaseModifier"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(uint id, bool checkInactive, Func<BaseModifier, bool>? predicate=null)
    {
        var type = ModifierManager.GetModifierType(id) ?? throw new InvalidOperationException(
            $"Cannot get modifier with id {id} because it is not registered.");

        return HasModifier(type, checkInactive, predicate);
    }

    /// <summary>
    /// Checks if a player has an active <see cref="BaseModifier"/> by its unique ID.
    /// </summary>
    /// <param name="id">The <see cref="BaseModifier"/>'s guid.</param>
    /// <param name="checkInactive">Whether to check inactive <see cref="BaseModifier"/>s (those pending to be added).</param>
    /// <returns><see langword="true"/> if the <see cref="BaseModifier"/> is present, <see langword="false"/> otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifier(Guid id, bool checkInactive)
    {
        return ActiveModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
        bool MatchExpr(BaseModifier bm) => bm.UniqueId == id;
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type, if the type is an interface.
    /// </summary>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <typeparam name="T">The Type of the interface of the Modifier.</typeparam>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifierOfType<T>(Func<T, bool>? predicate=null) where T : class
    {
        return ActiveModifiers.Exists(x => x is T modifier && (predicate == null || predicate(modifier)));
    }

    /// <summary>
    /// Checks if a player has an active modifier by its type, if the type is an interface.
    /// </summary>
    /// <param name="checkInactive">Whether to check inactive modifiers (those pending to be added).</param>
    /// <param name="predicate">The predicate to check the modifier.</param>
    /// <typeparam name="T">The Type of the interface of the Modifier.</typeparam>
    /// <returns>True if the Modifier is present, false otherwise.</returns>
    [HideFromIl2Cpp]
    public bool HasModifierOfType<T>(bool checkInactive, Func<T, bool>? predicate=null) where T : class
    {
        return ActiveModifiers.Exists(MatchExpr) || (checkInactive && _toAdd.Exists(MatchExpr));
        bool MatchExpr(BaseModifier bm) => bm is T modifier && (predicate == null || predicate(modifier));
    }
}

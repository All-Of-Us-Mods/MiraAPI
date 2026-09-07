using System;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Modifiers;

/// <summary>
/// Base class for all modifiers.
/// </summary>
public abstract class BaseModifier : IOptionable
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that the modifier is attached to.
    /// </summary>
    public PlayerControl Player { get; internal set; } = null!;

    /// <summary>
    /// Gets the <see cref="Modifiers.ModifierComponent"/> that the modifier is attached to.
    /// </summary>
    public ModifierComponent? ModifierComponent { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the modifier has been initialized.
    /// </summary>
    public bool Initialized { get; internal set; }

    /// <summary>
    /// Gets the unique ID of the modifier.
    /// </summary>
    public Guid UniqueId { get; internal set; } = Guid.Empty;

    /// <summary>
    /// Gets the type ID of the modifier.
    /// </summary>
    public uint TypeId => ModifierManager.GetModifierTypeId(GetType()) ?? throw new InvalidOperationException("Modifier is not registered.");

    /// <summary>
    /// Gets the parent mod of the modifier.
    /// </summary>
    public MiraPluginInfo ParentMod => Array.Find(
        MiraPluginManager.Instance.RegisteredPlugins,
        x => x.InternalModifiers.Exists(y => y.TypeId == TypeId)
        ) ?? throw new InvalidOperationException("Modifier is not registered.");

    /// <summary>
    /// Gets the modifier name.
    /// </summary>
    public abstract string ModifierName { get; }

    /// <summary>
    /// Gets the modifier icon. Useless if <see cref="HideOnUi"/> is <see langword="true"/>.
    /// </summary>
    public virtual LoadableAsset<Sprite>? ModifierIcon => null;

    /// <summary>
    /// Gets a value indicating whether the modifier is hidden on the UI. Will be hidden either way if no description is provided.
    /// </summary>
    public virtual bool HideOnUi => GetDescription() == string.Empty;

    /// <summary>
    /// Gets a value indicating whether the modifier is shown in the freeplay menu.
    /// </summary>
    public virtual bool ShowInFreeplay => false;

    /// <summary>
    /// Gets a value indicating the <see cref="Color"/> that should be used for the modifier within freeplay.
    /// </summary>
    public virtual Color FreeplayFileColor => Color.gray;

    /// <summary>
    /// Gets a value indicating whether the modifier is unique. If <see langword="true"/>, the player can only have one instance of this modifier.
    /// </summary>
    public virtual bool Unique => true;

    /// <summary>
    /// Gets the HUD description for this modifier. Does nothing if <see cref="HideOnUi"/> is <see langword="true"/>. Required to be visible on UI.
    /// </summary>
    /// <returns>The description string for the HUD.</returns>
    public virtual string GetDescription() => string.Empty;

    /// <summary>
    /// Called when the modifier is activated.
    /// </summary>
    public virtual void OnActivate()
    {
    }

    /// <summary>
    /// Called when the modifier is deactivated.
    /// </summary>
    public virtual void OnDeactivate()
    {
    }

    /// <summary>
    /// Called when the modifier is updated. Attached to <see cref="ModifierComponent"/>'s <see cref="ModifierComponent.Update"/> method.
    /// </summary>
    public virtual void Update()
    {
    }

    /// <summary>
    /// Called when the modifier is updated. Attached to <see cref="ModifierComponent"/>'s <see cref="ModifierComponent.FixedUpdate"/> method.
    /// </summary>
    public virtual void FixedUpdate()
    {
    }

    /// <summary>
    /// Called when the player dies.
    /// </summary>
    /// <param name="reason">The Death Reason.</param>
    public virtual void OnDeath(DeathReason reason)
    {
    }

    /// <summary>
    /// Called when a meeting starts.
    /// </summary>
    public virtual void OnMeetingStart()
    {
    }

    /// <summary>
    /// Determines whether the player can vent.
    /// </summary>
    /// <returns><see langword="true"/> if the player can vent, <see langword="false"/> otherwise. <see langword="null"/> for no effect.</returns>
    public virtual bool? CanVent() => null;

    /// <summary>
    /// Removes this modifier instance from the player.
    /// </summary>
    public void RemoveSelf()
    {
        ModifierComponent?.RemoveModifier(this);
    }
}

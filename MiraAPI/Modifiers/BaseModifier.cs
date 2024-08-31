namespace MiraAPI.Modifiers;

/// <summary>
/// Base class for all modifiers.
/// </summary>
public abstract class BaseModifier
{
    /// <summary>
    /// Gets the player that the modifier is attached to.
    /// </summary>
    public PlayerControl? Player { get; internal set; }

    /// <summary>
    /// Gets the modifier id.
    /// </summary>
    public uint ModifierId { get; internal set; }

    /// <summary>
    /// Gets the modifier name.
    /// </summary>
    public abstract string ModifierName { get; }

    /// <summary>
    /// Gets a value indicating whether the modifier is hidden on the UI.
    /// </summary>
    public virtual bool HideOnUi => false;

    /// <summary>
    /// Gets the HUD information for this modifier. Defaults to the modifier name. Does nothing if <see cref="HideOnUi"/> is true.
    /// </summary>
    /// <returns>The information string for the HUD.</returns>
    public virtual string GetHudString() => ModifierName;

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
    /// Called when the modifier is updated. Attached to the ModifierComponent's Update method.
    /// </summary>
    public virtual void Update()
    {
    }

    /// <summary>
    /// Called when the modifier is updated. Attached to the ModifierComponent's FixedUpdate method.
    /// </summary>
    public virtual void FixedUpdate()
    {
    }

    /// <summary>
    /// Called when the player dies.
    /// </summary>
    public virtual void OnDeath()
    {
    }

    /// <summary>
    /// Determines whether the player can vent.
    /// </summary>
    /// <returns>True if the player can vent, false otherwise.</returns>
    public virtual bool CanVent() => Player?.Data.Role.CanVent == true;
}

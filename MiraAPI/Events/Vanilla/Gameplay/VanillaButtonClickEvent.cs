using MiraAPI.Hud;

namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Button click event for Vanilla <see cref="AbilityButton"/> only. Do not use for <see cref="CustomActionButton"/>s.
/// </summary>
/// <param name="button">The Vanilla <see cref="AbilityButton"/> that was clicked.</param>
/// <param name="playerTarget">The <see cref="PlayerControl"/> target, if any.</param>
/// <param name="ventTarget">The <see cref="Vent"/> target, if any.</param>
public sealed class VanillaButtonClickEvent(AbilityButton button, PlayerControl? playerTarget = null, Vent? ventTarget = null) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the Vanilla <see cref="AbilityButton"/> that was clicked.
    /// </summary>
    public AbilityButton Button => button;

    /// <summary>
    /// Gets the target <see cref="PlayerControl"/>, if any, of the Vanilla <see cref="AbilityButton"/> that was clicked.
    /// </summary>
    public PlayerControl? PlayerTarget => playerTarget;

    /// <summary>
    /// Gets the target <see cref="Vent"/>, if any, of the Vanilla <see cref="AbilityButton"/> that was clicked.
    /// </summary>
    public Vent? VentTarget => ventTarget;
}

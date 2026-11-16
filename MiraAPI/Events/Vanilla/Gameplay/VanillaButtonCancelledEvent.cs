using MiraAPI.Hud;

namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Invoked when a Vanilla <see cref="AbilityButton"/> click is cancelled. Do not use for <see cref="CustomActionButton"/>s.
/// </summary>
/// <param name="button">The vanilla <see cref="AbilityButton"/> whose click was cancelled.</param>
public sealed class VanillaButtonCancelledEvent(AbilityButton button) : MiraEvent
{
    /// <summary>
    /// Gets the Vanilla <see cref="AbilityButton"/> whose click was cancelled.
    /// </summary>
    public AbilityButton Button => button;
}

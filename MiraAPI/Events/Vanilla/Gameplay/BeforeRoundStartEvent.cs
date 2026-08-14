namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event invoked on <see cref="IntroCutscene.OnDestroy"/> and <see cref="ExileController.WrapUp"/> to determine if it should run the <see cref="RoundStartEvent"/> event or not.
/// </summary>
/// <param name="triggeredByIntro">Whether the event was triggered by the intro or not.</param>
public class BeforeRoundStartEvent(bool triggeredByIntro) : MiraCancelableEvent
{
    /// <summary>
    /// Gets a value indicating whether the event was triggered by the <see cref="IntroCutscene"/> or <see cref="ExileController"/>.
    /// </summary>
    public bool TriggeredByIntro { get; } = triggeredByIntro;
}

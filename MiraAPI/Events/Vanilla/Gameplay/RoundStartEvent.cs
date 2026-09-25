namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Start Round event, invoked on <see cref="IntroCutscene.OnDestroy"/> and <see cref="ExileController.WrapUp"/>
/// if the <see cref="BeforeRoundStartEvent"/> isn't cancelled.
/// </summary>
/// <param name="triggeredByIntro">Whether the event was triggered by the intro or not.</param>
public class RoundStartEvent(bool triggeredByIntro) : MiraEvent
{
    /// <summary>
    /// Gets a value indicating whether the event was triggered by the <see cref="IntroCutscene"/> or <see cref="ExileController"/>.
    /// </summary>
    public bool TriggeredByIntro { get; } = triggeredByIntro;
}

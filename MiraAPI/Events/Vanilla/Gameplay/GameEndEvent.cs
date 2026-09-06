namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// The event that is invoked when the end game screen is shown. Non-cancelable.
/// </summary>
/// <param name="manager">The <see cref="global::EndGameManager"/> instance.</param>
public class GameEndEvent(EndGameManager manager) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="global::EndGameManager"/> instance.
    /// </summary>
    public EndGameManager EndGameManager { get; } = manager;
}

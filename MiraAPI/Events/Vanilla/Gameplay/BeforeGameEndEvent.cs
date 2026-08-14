namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Invoked before <see cref="GameManager.RpcEndGame"/> is called, allowing cancellation of the game end entirely.
/// </summary>
/// <param name="reason">The reason for the game end.</param>
public class BeforeGameEndEvent(GameOverReason reason) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the reason for the game end.
    /// </summary>
    public GameOverReason Reason { get; } = reason;
}

namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// The event that is invoked when a player completes a task within Hide and Seek. Non-cancelable.
/// </summary>
/// <param name="hnsManager">The <see cref="HideAndSeekManager"/> currently active.</param>
/// <param name="player">The player who completed the task.</param>
/// <param name="task">The task that the player completed.</param>
public class CompleteHnsTaskEvent(HideAndSeekManager hnsManager, PlayerControl player, PlayerTask task) : MiraEvent
{
    /// <summary>
    /// Gets the instance of the <see cref="PlayerControl"/>.
    /// </summary>
    public PlayerControl Player { get; } = player;

    /// <summary>
    /// Gets the instance of the <see cref="PlayerTask"/> that the player completed.
    /// </summary>
    public PlayerTask Task { get; } = task;

    /// <summary>
    /// Gets the instance of the <see cref="HideAndSeekManager"/> attached to the game.
    /// </summary>
    public HideAndSeekManager HnsManager { get; } = hnsManager;
}

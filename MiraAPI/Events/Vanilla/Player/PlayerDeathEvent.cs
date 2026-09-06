namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// Event that is invoked when a player dies. Non-cancelable.
/// </summary>
/// <param name="player">The player who died.</param>
/// <param name="reason">The reason the player died.</param>
/// <param name="deadBody">The player's <see cref="global::DeadBody"/>, if it exists.</param>
public class PlayerDeathEvent(PlayerControl player, DeathReason reason, DeadBody? deadBody) : MiraEvent
{
    /// <summary>
    /// Gets the player who died.
    /// </summary>
    public PlayerControl Player { get; } = player;

    /// <summary>
    /// Gets the reason the player died.
    /// </summary>
    public DeathReason DeathReason { get; } = reason;

    /// <summary>
    /// Gets the <see cref="global::DeadBody"/> associated with the player, if any.
    /// </summary>
    public DeadBody? DeadBody { get; } = deadBody;
}

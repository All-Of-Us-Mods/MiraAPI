namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event that is invoked after a player is murdered. Only called after a successful murder. This event is not cancelable.
/// </summary>
/// <param name="source">The killer.</param>
/// <param name="target">The killed <see cref="PlayerControl"/>.</param>
/// <param name="deadBody">The <paramref name="target"/>'s <see cref="global::DeadBody"/>, if it exists.</param>
public class AfterMurderEvent(PlayerControl source, PlayerControl target, DeadBody? deadBody) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that killed the <see cref="Target"/>.
    /// </summary>
    public PlayerControl Source { get; } = source;

    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that was killed.
    /// </summary>
    public PlayerControl Target { get; } = target;

    /// <summary>
    /// Gets the <see cref="Target"/>'s <see cref="global::DeadBody"/>, if it exists.
    /// </summary>
    public DeadBody? DeadBody { get; } = deadBody;
}

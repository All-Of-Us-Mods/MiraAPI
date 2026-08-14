namespace MiraAPI.Events.Vanilla.Usables;

/// <summary>
/// Event that is invoked when a player exits a vent. This event is cancelable.
/// </summary>
/// <param name="player">The <see cref="PlayerControl"/> who is exiting the <paramref name="vent"/>.</param>
/// <param name="vent">The <see cref="global::Vent"/> being exited from.</param>
public class ExitVentEvent(PlayerControl player, Vent? vent) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that is exiting the <see cref="Vent"/>.
    /// </summary>
    public PlayerControl Player { get; } = player;

    /// <summary>
    /// Gets the <see cref="global::Vent"/> that the <see cref="Player"/> is exiting.
    /// </summary>
    public Vent? Vent { get; } = vent;
}

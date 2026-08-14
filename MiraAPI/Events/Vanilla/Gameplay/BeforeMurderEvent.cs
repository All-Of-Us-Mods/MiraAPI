using MiraAPI.Networking;

namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event that is invoked before a player is murdered. This event is cancelable.
/// </summary>
/// <param name="source">The <see cref="PlayerControl"/> that is killing the <paramref name="target"/>.</param>
/// <param name="target">The <see cref="PlayerControl"/> that is being killed.</param>
/// <param name="inMeeting">Whether the murder is intended to be triggered in a meeting.</param>
public sealed class BeforeMurderEvent(PlayerControl source, PlayerControl target, MeetingCheck inMeeting) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that is killing the <see cref="Target"/>.
    /// </summary>
    public PlayerControl Source { get; } = source;

    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that is being killed.
    /// </summary>
    public PlayerControl Target { get; } = target;

    /// <summary>
    /// Gets whether the murder was meant to be done in a meeting, via an enum.
    /// </summary>
    public MeetingCheck InMeeting { get; } = inMeeting;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeMurderEvent"/> class.
    /// </summary>
    /// <param name="source">The <see cref="PlayerControl"/> that is killing the <paramref name="target"/>.</param>
    /// <param name="target">The <see cref="PlayerControl"/> that is being killed.</param>
    public BeforeMurderEvent(PlayerControl source, PlayerControl target) : this(source, target, MeetingCheck.Ignore)
    {
    }
}

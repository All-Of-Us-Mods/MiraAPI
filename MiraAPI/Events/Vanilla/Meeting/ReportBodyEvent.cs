namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when a body is reported.
/// This is invoked in <see cref="PlayerControl.CmdReportDeadBody(NetworkedPlayerInfo)"/>.
/// </summary>
/// <param name="reporter">The player who reported the body.</param>
/// <param name="target">The player being reported.</param>
/// <param name="body">The <see cref="DeadBody"/> being reported.</param>
public class ReportBodyEvent(PlayerControl reporter, NetworkedPlayerInfo? target, DeadBody? body) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the player that is reporting the body.
    /// </summary>
    public PlayerControl Reporter { get; } = reporter;

    /// <summary>
    /// Gets the player that is being reported. Will be <see langword="null"/> for emergency meeting.
    /// </summary>
    public NetworkedPlayerInfo? Target { get; } = target;

    /// <summary>
    /// Gets the <see cref="DeadBody"/> that is being reported. Will be <see langword="null"/> for emergency meeting.
    /// </summary>
    public DeadBody? Body { get; } = body;
}

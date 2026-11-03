namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when the meeting has ended.
/// </summary>
/// <param name="meetingHud">The meeting hud of the meeting that invoked the event.</param>
public class EndMeetingEvent(MeetingHud meetingHud) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="global::MeetingHud"/> instance.
    /// </summary>
    public MeetingHud MeetingHud { get; } = meetingHud;
}

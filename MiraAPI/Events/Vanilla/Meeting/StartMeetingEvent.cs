namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when a meeting is called. This event is not cancelable.
/// This event is called after Mira resets votes, so if you plan on adding votes to a specific player, do it with this event.
/// </summary>
/// <param name="meetingHud">The <see cref="global::MeetingHud"/> instance.</param>
public class StartMeetingEvent(MeetingHud meetingHud) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="global::MeetingHud"/> instance.
    /// </summary>
    public MeetingHud MeetingHud { get; } = meetingHud;
}

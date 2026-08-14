namespace MiraAPI.Events.Vanilla.Map;

/// <summary>
/// Event fired when a player closes the doors in a room.
/// </summary>
/// <param name="room">The room that was closed.</param>
public class CloseDoorsEvent(SystemTypes room) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the room that the doors were closed in.
    /// </summary>
    public SystemTypes Room { get; } = room;
}

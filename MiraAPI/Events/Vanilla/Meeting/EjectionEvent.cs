namespace MiraAPI.Events.Vanilla.Meeting;

/// <summary>
/// The event that is invoked when a player is ejected. Non-cancelable.
/// </summary>
/// <param name="controller">The <see cref="global::ExileController"/> instance.</param>
public class EjectionEvent(ExileController controller) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="global::ExileController"/> instance.
    /// </summary>
    public ExileController ExileController { get; } = controller;
}

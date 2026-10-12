namespace MiraAPI.Events.Mira;

/// <summary>
/// Event fired by Mira API prior to adjusting the UI buttons at the top right of the screen.
/// </summary>
public class UiButtonResetEvent : MiraEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UiButtonResetEvent"/> class.
    /// </summary>
    public UiButtonResetEvent()
    {
        // nothing!
    }
}

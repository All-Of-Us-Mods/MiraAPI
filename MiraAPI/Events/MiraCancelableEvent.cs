using MiraAPI.PluginLoading;

namespace MiraAPI.Events;

/// <summary>
/// Abstract class for Mira Events that can be cancelled.
/// </summary>
[MiraIgnore]
public abstract class MiraCancelableEvent : MiraEvent
{
    /// <summary>
    /// Gets a value indicating whether the event is cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Cancels the event.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }

    /// <summary>
    /// Uncancels the event.
    /// </summary>
    public void UnCancel()
    {
        IsCancelled = false;
    }
}

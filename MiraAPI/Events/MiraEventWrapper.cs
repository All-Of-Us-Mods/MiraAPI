using System;

namespace MiraAPI.Events;

/// <summary>
/// Wrapper for <see cref="MiraEvent"/> handlers.
/// </summary>
/// <param name="eventHandler">The action for the event handler.</param>
/// <param name="priority">The priority of the handler.</param>
public class MiraEventWrapper(Delegate eventHandler, int priority)
{
    /// <summary>
    /// Gets the event handler delegate.
    /// </summary>
    public Delegate EventHandler { get; } = eventHandler;

    /// <summary>
    /// Gets the priority of the handler. Lower values are called first.
    /// </summary>
    public int Priority { get; } = priority;
}

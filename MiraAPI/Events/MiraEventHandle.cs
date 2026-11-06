using System;

namespace MiraAPI.Events;

/// <summary>
/// Wrapper for <see cref="MiraEvent"/> handlers.
/// Also used when unregistering a <see cref="MiraEvent"/>.
/// </summary>
public class MiraEventHandle
{
    /// <summary>
    /// Gets the type of event.
    /// </summary>
    public Type EventType { get; }

    /// <summary>
    /// Gets the priority of the handler. Lower values are called first.
    /// </summary>
    internal int Priority { get; }

    /// <summary>
    /// Gets the event handler delegate.
    /// </summary>
    internal Delegate EventHandler { get; }

    internal MiraEventHandle(Delegate eventHandler, int priority, Type type)
    {
        EventHandler = eventHandler;
        Priority = priority;
        EventType = type;
    }
}

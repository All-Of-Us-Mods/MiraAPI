using System;

namespace MiraAPI.Events;

/// <summary>
/// Wrapper for <see cref="MiraEvent"/> handlers.
/// </summary>
public class MiraEventWrapper
{
    /// <summary>
    /// Gets the event handler delegate.
    /// </summary>
    public Delegate EventHandler { get; }

    /// <summary>
    /// Gets the priority of the handler. Lower values are called first.
    /// </summary>
    public int Priority { get; }

    internal Type DefinedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraEventWrapper"/> class.
    /// </summary>
    /// <param name="eventHandler">The action for the event handler.</param>
    /// <param name="priority">The priority of the handler.</param>
    /// <param name="type">The constructed event type.</param>
    internal MiraEventWrapper(Delegate eventHandler, int priority, Type type)
    {
        EventHandler = eventHandler;
        Priority = priority;
        DefinedType = type;
    }
}

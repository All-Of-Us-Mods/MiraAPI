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
    public int Priority { get; }

    /// <summary>
    /// Gets the event handler delegate.
    /// </summary>
    public Delegate EventHandler { get; }

    /// <summary>
    /// Gets a value indicating whether the event handler should also be
    /// triggered by subclasses of <see cref="EventType"/>.
    /// </summary>
    public bool InvokeOnSubtype { get; }

    internal MiraEventHandle(Delegate eventHandler, int priority, Type type, bool invokeOnSubtype)
    {
        EventHandler = eventHandler;
        Priority = priority;
        EventType = type;
        InvokeOnSubtype = invokeOnSubtype;
    }

    internal bool IsTriggeredBy(Type type)
    {
        if (InvokeOnSubtype)
        {
            return EventType.IsAssignableTo(type);
        }

        return EventType == type;
    }
}

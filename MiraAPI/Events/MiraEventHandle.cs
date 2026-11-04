using System;

namespace MiraAPI.Events;

/// <summary>
/// Event handle to use when unregistering a <see cref="MiraEvent"/>.
/// </summary>
public class MiraEventHandle
{
    /// <summary>
    /// Gets the type of event.
    /// </summary>
    public Type EventType => EventWrapper.DefinedType;

    internal MiraEventWrapper EventWrapper { get; }

    internal MiraEventHandle(MiraEventWrapper eventWrapper)
    {
        EventWrapper = eventWrapper;
    }
}

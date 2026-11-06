using System;

namespace MiraAPI.Events;

/// <summary>
/// Wrapper for <see cref="MiraEvent"/> handlers.
/// </summary>
/// <param name="eventHandler">The action for the event handler.</param>
/// <param name="priority">The priority of the handler.</param>
/// <param name="type">The constructed event type.</param>
internal class MiraEventWrapper(Delegate eventHandler, int priority, Type type)
{
    /// <summary>
    /// Gets the event handler delegate.
    /// </summary>
    public Delegate EventHandler { get; } = eventHandler;

    /// <summary>
    /// Gets the priority of the handler. Lower values are called first.
    /// </summary>
    public int Priority { get; } = priority;

    /// <summary>
    /// Gets the <see cref="MiraEvent"/> type used to construct the <see cref="EventHandler"/>.
    /// </summary>
    public Type DefinedType { get; } = type;
}

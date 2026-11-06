using System;

namespace MiraAPI.Events;

/// <summary>
/// Register a <see cref="MiraEvent"/> handler.
/// </summary>
/// <param name="priority">The priority of the event. Lower values are called first.</param>
/// <param name="invokeOnSubtype">Whether the event handler should also be triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.</param>
[AttributeUsage(AttributeTargets.Method)]
public class RegisterEventAttribute(int priority = 0, bool invokeOnSubtype = true) : Attribute
{
    /// <summary>
    /// Gets the priority of the event. Lower values are called first.
    /// </summary>
    public int Priority { get; } = priority;

    /// <summary>
    /// Gets a value indicating whether the event handler should also be
    /// triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.
    /// </summary>
    public bool InvokeOnSubtype { get; } = invokeOnSubtype;
}

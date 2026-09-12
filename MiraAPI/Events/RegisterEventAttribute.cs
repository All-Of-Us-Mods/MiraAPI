using System;

namespace MiraAPI.Events;

/// <summary>
/// Register a <see cref="MiraEvent"/> handler.
/// </summary>
/// <param name="priority">The priority of the event. Lower values are called first.</param>
[AttributeUsage(AttributeTargets.Method)]
public class RegisterEventAttribute(int priority = 0) : Attribute
{
    /// <summary>
    /// Gets the priority of the event. Lower values are called first.
    /// </summary>
    public int Priority { get; } = priority;
}

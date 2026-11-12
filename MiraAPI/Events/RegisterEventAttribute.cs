using System;

namespace MiraAPI.Events;

/// <summary>
/// Register a <see cref="MiraEvent"/> handler.
/// </summary>
/// <param name="priority">The priority of the event. Lower values are called first.</param>
/// <param name="invokeOnSubtype">Whether the event handler should also be triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.</param>
[AttributeUsage(AttributeTargets.Method)]
public class RegisterEventAttribute(int priority, bool invokeOnSubtype) : Attribute
{
    /// <summary>
    /// Gets the priority of the event. Lower values are called first.
    /// </summary>
    public int Priority => priority;

    /// <summary>
    /// Gets a value indicating whether the event handler should also be
    /// triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.
    /// </summary>
    public bool InvokeOnSubtype => invokeOnSubtype;

    /// <inheritdoc cref="RegisterEventAttribute"/>
    public RegisterEventAttribute(int priority)
        : this(priority, false)
    {
    }

    /// <inheritdoc cref="RegisterEventAttribute"/>
    public RegisterEventAttribute(bool invokeOnSubtype)
        : this(0, invokeOnSubtype)
    {
    }

    /// <inheritdoc cref="RegisterEventAttribute"/>
    public RegisterEventAttribute()
        : this(0, false)
    {
    }
}

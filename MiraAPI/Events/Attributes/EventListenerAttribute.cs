using System;

namespace MiraAPI.Events.Attributes;

/// <summary>
/// Used to listen for events.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EventListenerAttribute(string modId = "") : Attribute
{
    /// <summary>
    /// Gets the mod ID of the event being handled.
    /// </summary>
    public string ModId { get; } = modId;
}

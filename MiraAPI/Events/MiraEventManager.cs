using MiraAPI.PluginLoading;
using Reactor.Utilities;
using System;
using System.Collections.Generic;

namespace MiraAPI.Events;
public static class MiraEventManager
{
    internal static readonly List<AbstractEvent> Events = [];

    internal static void RegisterEvent(Type eventType, MiraPluginInfo pluginInfo)
    {
        if (!typeof(AbstractEvent).IsAssignableFrom(eventType))
        {
            Logger<MiraApiPlugin>.Error($"{eventType?.Name} does not inherit from IEvent.");
            return;
        }

        if (Activator.CreateInstance(eventType) is not AbstractEvent abstractEvent)
        {
            Logger<MiraApiPlugin>.Error($"Failed to create event from {eventType.Name}");
            return;
        }

        Events.Add(abstractEvent);
        pluginInfo.Events.Add(abstractEvent);
    }

    /// <summary>
    /// Invoke events.
    /// </summary>
    /// <param name="targetEvent">The event you want to invoke.</param>
    /// <param name="invocationType">Which mods the event listeners will be called.</param>
    public static void InvokeEvent(AbstractEvent targetEvent, EventInvocationType invocationType = EventInvocationType.AllMods)
    {
        // TODO: Add code.
    }
}

/// <summary>
/// Which mods the event listeners will be called.
/// </summary>
public enum EventInvocationType
{
    /// <summary>
    /// Only the mod invoking the command.
    /// </summary>
    SendingMod,

    /// <summary>
    /// All mods that implement this event.
    /// </summary>
    AllMods,
}
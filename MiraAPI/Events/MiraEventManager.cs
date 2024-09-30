using MiraAPI.PluginLoading;
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using MiraAPI.Events.Attributes;

namespace MiraAPI.Events;

/// <summary>
/// Used internally to register events, and externally to invoke events.
/// </summary>
public static class MiraEventManager
{
    internal static readonly List<AbstractEvent> Events = [];
    internal static readonly Dictionary<EventListenerAttribute, AbstractEvent> Listeners = [];

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
        if (!Events.Contains(targetEvent))
        {
            Logger<MiraApiPlugin>.Error($"Can't invoke {targetEvent.GetType().Name} because it is not registered.");
            return;
        }

        foreach (var pair in Listeners)
        {
            if (pair.Value != targetEvent) continue;
           // pair.Key.Method.Invoke(targetEvent);
        }
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
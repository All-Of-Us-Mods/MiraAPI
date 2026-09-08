using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.Events;

/// <summary>
/// Mira Event manager.
/// </summary>
public static class MiraEventManager
{
    private static readonly Dictionary<Type, List<MiraEventWrapper>> EventWrappers = [];

    /// <summary>
    /// Invoke a <see cref="MiraEvent"/>.
    /// </summary>
    /// <param name="eventInstance">The <typeparamref name="T"/> instance.</param>
    /// <typeparam name="T">Type of Event.</typeparam>
    /// <returns>If there was an event handler invoked for this event, return <see langword="true"/>. Otherwise, return <see langword="false"/>.</returns>
    public static bool InvokeEvent<T>(this T eventInstance) where T : MiraEvent
    {
        EventWrappers.TryGetValue(typeof(T), out var handlers);
        if (handlers == null || handlers.Count == 0)
        {
            return false;
        }

        foreach (var handler in handlers)
        {
            try
            {
                ((Action<T>)handler.EventHandler).Invoke(eventInstance);
            }
            catch (Exception ex)
            {
                Error($"Error invoking event handler for {typeof(T).Name}: {ex.ToString()}");
            }
        }

        return true;
    }

    /// <summary>
    /// Invoke a <see cref="MiraEvent"/> and use a specific type to find the handlers.
    /// </summary>
    /// <param name="eventInstance">The <see cref="MiraEvent"/> instance.</param>
    /// <param name="type">The type to use for handler lookup.</param>
    /// <returns>If there was an event handler invoked for this event, return <see langword="true"/>. Otherwise, return <see langword="false"/>.</returns>
    public static bool InvokeEvent(this MiraEvent eventInstance, Type type)
    {
        EventWrappers.TryGetValue(type, out var handlers);
        if (handlers == null || handlers.Count == 0)
        {
            return false;
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler.EventHandler.DynamicInvoke(eventInstance);
            }
            catch (Exception ex)
            {
                Error($"Error invoking event handler for {type.Name}: {ex.ToString()}");
            }
        }

        return true;
    }

    /// <summary>
    /// Register a <see cref="MiraEvent"/> handler.
    /// </summary>
    /// <param name="type">The type of <see cref="MiraEvent"/> event.</param>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> of the event handler.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    public static MiraEventHandle RegisterEventHandler(Type type, MethodInfo methodInfo, int priority = 0)
    {
        if (!type.IsSubclassOf(typeof(MiraEvent)))
        {
            throw new InvalidOperationException($"Type must be a subclass of MiraEvent: {type.FullName}");
        }

        EventWrappers.TryAdd(type, []);
        var handlers = EventWrappers[type];

        var @delegate = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(type), methodInfo);
        var eventWrapper = new MiraEventWrapper(@delegate, priority);

        var index = handlers.BinarySearch(eventWrapper, Comparer<MiraEventWrapper>.Create((a, b) => a.Priority.CompareTo(b.Priority)));

        if (index < 0)
        {
            index = ~index;
        }

        handlers.Insert(index, eventWrapper);
        return new MiraEventHandle(type, eventWrapper);
    }

    /// <summary>
    /// Register a <typeparamref name="T"/> handler.
    /// </summary>
    /// <param name="handler">The callback method/handler for the event.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <typeparam name="T">Type of <see cref="MiraEvent"/> event.</typeparam>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    public static MiraEventHandle RegisterEventHandler<T>(Action<T> handler, int priority = 0) where T : MiraEvent
    {
        EventWrappers.TryAdd(typeof(T), []);

        var handlers = EventWrappers[typeof(T)];
        var eventWrapper = new MiraEventWrapper(handler, priority);

        var index = handlers.BinarySearch(eventWrapper, Comparer<MiraEventWrapper>.Create((a, b) => a.Priority.CompareTo(b.Priority)));

        if (index < 0)
        {
            index = ~index;
        }

        handlers.Insert(index, eventWrapper);
        return new MiraEventHandle(typeof(T), eventWrapper);
    }

    /// <summary>
    /// Unregister a <see cref="MiraEvent"/> handler using the <see cref="MiraEventHandle"/>.
    /// </summary>
    /// <param name="eventHandle">A handle to the event.</param>
    /// <returns><see langword="true"/> if the event was unregistered successfully, <see langword="false"/> otherwise.</returns>
    public static bool UnregisterEventHandler(MiraEventHandle eventHandle)
    {
        if (!EventWrappers.TryGetValue(eventHandle.EventType, out var handlers))
        {
            return false;
        }

        if (!handlers.Remove(eventHandle.EventWrapper))
        {
            return false;
        }

        if (handlers.Count == 0)
        {
            EventWrappers.Remove(eventHandle.EventType);
        }
        return true;
    }
}

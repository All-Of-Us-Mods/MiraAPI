using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MiraAPI.PluginLoading;

namespace MiraAPI.Events;

/// <summary>
/// Mira Event manager.
/// </summary>
public static class MiraEventManager
{
    private static readonly Dictionary<Type, List<MiraEventWrapper>> EventWrappersToLoad = [];

    private static readonly Dictionary<Type, List<MiraEventWrapper>> EventWrappers = [];

    /// <summary>
    /// Gets the list of all registered <see cref="MiraEvent"/> types.
    /// </summary>
    public static IReadOnlyList<Type> EventTypes { get; internal set; } = [];

    internal static List<Type> InternalEventTypes { get; } = [];

    private static readonly Dictionary<Type, HashSet<Type>> TrackedEventTypes = [];

    private static bool loadFinilized = false;

    private static bool TryGetEventHandlers(Type type, [NotNullWhen(true)] out List<MiraEventWrapper>? eventHandlers)
    {
        eventHandlers = null;
        if (!EventWrappers.TryGetValue(type.GetTrackedType(), out var handlers))
        {
            return false;
        }

        var callableHandlers = handlers.Where(x => IsSubclassOf(type, x.DefinedType)).ToList();
        if (callableHandlers.Count == 0)
        {
            return false;
        }

        eventHandlers = callableHandlers;
        return true;
    }

    /// <summary>
    /// Invoke a <see cref="MiraEvent"/>.
    /// </summary>
    /// <param name="eventInstance">The <typeparamref name="T"/> instance.</param>
    /// <typeparam name="T">Type of Event.</typeparam>
    /// <returns>If there was an event handler invoked for this event, return <see langword="true"/>. Otherwise, return <see langword="false"/>.</returns>
    public static bool InvokeEvent<T>(this T eventInstance)
        where T : MiraEvent
    {
        if (!TryGetEventHandlers(typeof(T), out var handlers))
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
                Error($"Error invoking event handler for {typeof(T).Name}: {ex}");
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
        if (!TryGetEventHandlers(type, out var handlers))
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
                Error($"Error invoking event handler for {type.Name}: {ex}");
            }
        }

        return true;
    }

    internal static bool RegisterEventType(Type eventType, MiraPluginInfo info)
    {
        if (!typeof(MiraEvent).IsAssignableFrom(eventType))
        {
            return false;
        }

        AddTypeToTracked(eventType);

        TrackedEventTypes[eventType].UnionWith(InternalEventTypes.Where(x => IsSubclassOf(eventType, x)));

        foreach (var subType in InternalEventTypes.Where(x => IsSubclassOf(x, eventType)))
        {
            AddTypeToTracked(subType, eventType);
        }

        info.InternalEventTypes.Add(eventType);
        InternalEventTypes.Add(eventType);

        return true;
    }

    private static void AddTypeToTracked(Type eventType, Type trackedAs = null!)
    {
        if (eventType.IsAbstract)
        {
            return;
        }

        if (!TrackedEventTypes.TryGetValue(eventType, out var trackedEvents))
        {
            trackedEvents = [];
            TrackedEventTypes.Add(eventType, trackedEvents);
        }

        trackedAs ??= eventType;
        trackedEvents.Add(trackedAs);
    }

    private static bool IsSubclassOf(Type? type, Type super)
    {
        if (type == null || type == typeof(MiraEvent))
        {
            return false;
        }

        if (type == super)
        {
            return true;
        }

        if (super.IsGenericTypeDefinition &&
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == super)
        {
            return true;
        }

        return IsSubclassOf(type.BaseType, super);
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

        var @delegate = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(type), methodInfo);
        return RegisterEventHandler(type, @delegate, priority);
    }

    /// <summary>
    /// Register a <typeparamref name="T"/> handler.
    /// </summary>
    /// <param name="handler">The callback method/handler for the event.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <typeparam name="T">Type of <see cref="MiraEvent"/> event.</typeparam>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    public static MiraEventHandle RegisterEventHandler<T>(Action<T> handler, int priority = 0)
        where T : MiraEvent
    {
        return RegisterEventHandler(typeof(T), handler, priority);
    }

    /// <summary>
    /// Register a <see cref="MiraEvent"/> handler.
    /// </summary>
    /// <param name="type">The type of <see cref="MiraEvent"/> event.</param>
    /// <param name="delegate">The <see cref="Delegate"/> method/handler for the event.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    private static MiraEventHandle RegisterEventHandler(Type type, Delegate @delegate, int priority)
    {
        var eventWrapper = new MiraEventWrapper(@delegate, priority, type);
        EventHandlerAdded(type, eventWrapper);
        return new MiraEventHandle(type, eventWrapper);
    }

    private static Type GetTrackedType(this Type type)
    {
        if (type.IsGenericType)
        {
            return type.GetGenericTypeDefinition();
        }

        return type;
    }

    private static void EventHandlerAdded(Type type, MiraEventWrapper eventWrapper)
    {
        if (!loadFinilized)
        {
            EventWrappersToLoad.TryAdd(type, []);
            EventWrappersToLoad[type].Add(eventWrapper);
            return;
        }

        if (!TrackedEventTypes.TryGetValue(type.GetTrackedType(), out var trackedEvents))
        {
            return;
        }

        foreach (Type eventType in trackedEvents)
        {
            EventWrappers.TryAdd(eventType, []);
            var handlers = EventWrappers[eventType];

            var index = handlers.BinarySearch(eventWrapper, Comparer<MiraEventWrapper>.Create((a, b) => a.Priority.CompareTo(b.Priority)));

            if (index < 0)
            {
                index = ~index;
            }

            handlers.Insert(index, eventWrapper);
        }
    }

    /// <summary>
    /// Unregister a <see cref="MiraEvent"/> handler using the <see cref="MiraEventHandle"/>.
    /// </summary>
    /// <param name="eventHandle">A handle to the event.</param>
    /// <returns><see langword="true"/> if the event was unregistered successfully, <see langword="false"/> otherwise.</returns>
    public static bool UnregisterEventHandler(MiraEventHandle eventHandle)
    {
        if (!loadFinilized)
        {
            if (!EventWrappersToLoad.TryGetValue(eventHandle.EventType, out var handlersToLoad))
            {
                return false;
            }

            if (!handlersToLoad.Remove(eventHandle.EventWrapper))
            {
                return false;
            }

            return true;
        }

        if (!TrackedEventTypes.TryGetValue(eventHandle.EventType.GetTrackedType(), out var eventTypes))
        {
            return false;
        }

        var removed = false;
        foreach (var type in eventTypes)
        {
            if (!EventWrappers.TryGetValue(type, out var handlers))
            {
                continue;
            }

            removed |= handlers.Remove(eventHandle.EventWrapper);
        }

        return removed;
    }

    internal static void OnLoadFinilize()
    {
        loadFinilized = true;
        EventTypes = new ReadOnlyCollection<Type>(InternalEventTypes);

        foreach (var eventType in EventTypes)
        {
            EventWrappers.Add(eventType, []);
        }

        foreach (var (concreteType, eventHandlers) in EventWrappersToLoad)
        {
            if (!TrackedEventTypes.TryGetValue(concreteType.GetTrackedType(), out var trackedEventTypes))
            {
                continue;
            }

            foreach (var eventType in trackedEventTypes)
            {
                EventWrappers[eventType].AddRange(eventHandlers);
            }
        }
    }
}

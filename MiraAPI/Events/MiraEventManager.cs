using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace MiraAPI.Events;

/// <summary>
/// Mira Event manager.
/// </summary>
public static class MiraEventManager
{
    private static readonly Dictionary<Type, List<MiraEventHandle>> EventHandlersToLoad = [];

    private static readonly Dictionary<Type, List<MiraEventHandle>> EventHandlers = [];

    /// <summary>
    /// Gets the list of all registered <see cref="MiraEvent"/> types.
    /// Generic event types are registered as their generic type definitions.
    /// </summary>
    public static IReadOnlyList<Type> EventTypes { get; internal set; } = [];

    internal static List<Type> InternalEventTypes { get; } = [];

    private static readonly Dictionary<Type, HashSet<Type>> TrackedEventTypes = [];

    private static bool loadFinilized = false;

    /// <summary>
    /// Invoke a <see cref="MiraEvent"/>.
    /// </summary>
    /// <param name="eventInstance">The <typeparamref name="T"/> instance.</param>
    /// <typeparam name="T">Type of Event.</typeparam>
    /// <returns>If there was an event handler invoked for this event, return <see langword="true"/>. Otherwise, return <see langword="false"/>.</returns>
    public static bool InvokeEvent<T>(this T eventInstance)
        where T : MiraEvent
    {
        return eventInstance.InvokeEvent(typeof(T));
    }

    /// <summary>
    /// Invoke a <see cref="MiraEvent"/> and use a specific type to find the handlers.
    /// </summary>
    /// <param name="eventInstance">The <see cref="MiraEvent"/> instance.</param>
    /// <param name="type">The type to use for handler lookup.</param>
    /// <returns>If there was an event handler invoked for this event, return <see langword="true"/>. Otherwise, return <see langword="false"/>.</returns>
    public static bool InvokeEvent(this MiraEvent eventInstance, Type type)
    {
        if (!EventHandlers.TryGetValue(type.GetTrackedType(), out var handlers))
        {
            return false;
        }

        var callableHandlers = handlers.Where(x => x.IsTriggeredBy(type)).ToList();
        if (callableHandlers.Count == 0)
        {
            return false;
        }

        foreach (var handler in callableHandlers)
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
        if (!eventType.IsSubclassOf(typeof(MiraEvent)))
        {
            return false;
        }

        AddTypeToTracked(eventType);

        // subtypes are added to their supertypes, regardless of which were added first
        TrackedEventTypes[eventType].UnionWith(InternalEventTypes.Where(x => x.IsSubclassOfWithGeneric(eventType)));
        foreach (var supertype in InternalEventTypes.Where(x => eventType.IsSubclassOfWithGeneric(x)))
        {
            AddTypeToTracked(supertype, eventType);
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

    /// <summary>
    /// Register a <see cref="MiraEvent"/> handler.
    /// </summary>
    /// <param name="type">The type of <see cref="MiraEvent"/> event.</param>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> of the event handler.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <param name="invokeOnSubtype">Whether the event handler should also be triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.</param>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    public static MiraEventHandle RegisterEventHandler(Type type, MethodInfo methodInfo, int priority, bool invokeOnSubtype)
    {
        if (!type.IsAssignableTo(typeof(MiraEvent)))
        {
            throw new InvalidOperationException($"Type must be a subclass of MiraEvent: {type.FullName}");
        }

        var @delegate = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(type), methodInfo);
        return RegisterEventHandler(type, @delegate, priority, invokeOnSubtype);
    }

    /// <inheritdoc cref="RegisterEventHandler(Type, MethodInfo, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler(Type type, MethodInfo methodInfo, int priority)
    {
        return RegisterEventHandler(type, methodInfo, priority, true);
    }

    /// <inheritdoc cref="RegisterEventHandler(Type, MethodInfo, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler(Type type, MethodInfo methodInfo, bool invokeOnSubtype)
    {
        return RegisterEventHandler(type, methodInfo, 0, invokeOnSubtype);
    }

    /// <inheritdoc cref="RegisterEventHandler(Type, MethodInfo, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler(Type type, MethodInfo methodInfo)
    {
        return RegisterEventHandler(type, methodInfo, 0, true);
    }

    /// <summary>
    /// Register a <see cref="MiraEvent"/> handler.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> of the event handler.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <param name="invokeOnSubtype">Whether the event handler should also be triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.</param>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    public static MiraEventHandle RegisterEventHandler(MethodInfo methodInfo, int priority, bool invokeOnSubtype)
    {
        return RegisterEventHandler(methodInfo.GetParameters()[0].ParameterType, methodInfo, priority, invokeOnSubtype);
    }

    /// <inheritdoc cref="RegisterEventHandler(MethodInfo, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler(MethodInfo methodInfo, int priority)
    {
        return RegisterEventHandler(methodInfo, priority, true);
    }

    /// <inheritdoc cref="RegisterEventHandler(MethodInfo, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler(MethodInfo methodInfo, bool invokeOnSubtype)
    {
        return RegisterEventHandler(methodInfo, 0, invokeOnSubtype);
    }

    /// <inheritdoc cref="RegisterEventHandler(MethodInfo, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler(MethodInfo methodInfo)
    {
        return RegisterEventHandler(methodInfo, 0, true);
    }

    /// <summary>
    /// Register a <typeparamref name="T"/> handler.
    /// </summary>
    /// <param name="handler">The callback method/handler for the event.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <param name="invokeOnSubtype">Whether the event handler should also be triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.</param>
    /// <typeparam name="T">Type of <see cref="MiraEvent"/> event.</typeparam>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    public static MiraEventHandle RegisterEventHandler<T>(Action<T> handler, int priority, bool invokeOnSubtype)
        where T : MiraEvent
    {
        return RegisterEventHandler(typeof(T), handler, priority, invokeOnSubtype);
    }

    /// <inheritdoc cref="RegisterEventHandler{T}(Action{T}, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler<T>(Action<T> handler, int priority)
        where T : MiraEvent
    {
        return RegisterEventHandler(handler, priority, true);
    }

    /// <inheritdoc cref="RegisterEventHandler{T}(Action{T}, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler<T>(Action<T> handler, bool invokeOnSubtype)
        where T : MiraEvent
    {
        return RegisterEventHandler(handler, 0, invokeOnSubtype);
    }

    /// <inheritdoc cref="RegisterEventHandler{T}(Action{T}, int, bool)"/>
    public static MiraEventHandle RegisterEventHandler<T>(Action<T> handler)
        where T : MiraEvent
    {
        return RegisterEventHandler(handler, 0, true);
    }

    /// <summary>
    /// Register a <see cref="MiraEvent"/> handler.
    /// </summary>
    /// <param name="type">The type of <see cref="MiraEvent"/> event.</param>
    /// <param name="delegate">The <see cref="Delegate"/> method/handler for the event.</param>
    /// <param name="priority">The priority of the event handler. Lower values are called first.</param>
    /// <param name="invokeOnSubtype">Whether the event handler should also be triggered by subclasses of the <see cref="MiraEvent"/> type of the handler.</param>
    /// <returns>A <see cref="MiraEventHandle"/> to use when unregistering the event.</returns>
    private static MiraEventHandle RegisterEventHandler(Type type, Delegate @delegate, int priority, bool invokeOnSubtype)
    {
        var eventHandler = new MiraEventHandle(@delegate, priority, type, invokeOnSubtype);
        EventHandlerAdded(type, eventHandler);
        return eventHandler;
    }

    private static Type GetTrackedType(this Type type)
    {
        if (type.IsGenericType)
        {
            return type.GetGenericTypeDefinition();
        }

        return type;
    }

    private static void EventHandlerAdded(Type type, MiraEventHandle eventHandler)
    {
        if (!loadFinilized)
        {
            EventHandlersToLoad.TryAdd(type, []);
            EventHandlersToLoad[type].Add(eventHandler);
            return;
        }

        if (!TrackedEventTypes.TryGetValue(type.GetTrackedType(), out var trackedEvents))
        {
            return;
        }

        foreach (Type eventType in trackedEvents)
        {
            var handlers = EventHandlers[eventType];

            var index = handlers.BinarySearch(eventHandler, Comparer<MiraEventHandle>.Create((a, b) => a.Priority.CompareTo(b.Priority)));

            if (index < 0)
            {
                index = ~index;
            }

            handlers.Insert(index, eventHandler);
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
            if (!EventHandlersToLoad.TryGetValue(eventHandle.EventType, out var handlersToLoad))
            {
                return false;
            }

            if (!handlersToLoad.Remove(eventHandle))
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
            if (!EventHandlers.TryGetValue(type, out var handlers))
            {
                continue;
            }

            removed |= handlers.Remove(eventHandle);
        }

        return removed;
    }

    internal static void OnLoadFinilize()
    {
        loadFinilized = true;
        EventTypes = new ReadOnlyCollection<Type>(InternalEventTypes);

        foreach (var eventType in EventTypes)
        {
            EventHandlers.Add(eventType, []);
        }

        foreach (var (concreteType, eventHandlers) in EventHandlersToLoad)
        {
            foreach (var eventHandler in eventHandlers)
            {
                EventHandlerAdded(concreteType, eventHandler);
            }

            eventHandlers.Clear();
        }

        EventHandlersToLoad.Clear();
    }

    internal static void RegisterEvents(Type type, IMiraPlugin miraPlugin)
    {
        if (!miraPlugin.ForceRegisterEvents && !type.IsDefined<RegisterEventContainerAttribute>())
        {
            return;
        }

        foreach (var method in type.GetMethods(MiraPluginManager.DeclaredStaticMembers))
        {
            if (!method.TryGetCustomAttribute<RegisterEventAttribute>(out var eventAttribute))
                continue;

            var parameters = method.GetParameters();

            if (parameters.Length != 1 || !parameters[0].ParameterType.IsSubclassOf(typeof(MiraEvent)))
            {
                Error($"Invalid event registration method {method.Name} in {type.Name}");
                continue;
            }

            RegisterEventHandler(method, eventAttribute.Priority, eventAttribute.InvokeOnSubtype);
        }
    }
}

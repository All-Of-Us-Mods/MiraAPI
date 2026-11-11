using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MiraAPI.Events.Mira;
using MiraAPI.PluginLoading;

namespace MiraAPI.Hud;

/// <summary>
/// Custom button manager for handling <see cref="CustomActionButton"/>s.
/// </summary>
public static class CustomButtonManager
{
    /// <summary>
    /// Gets a list of all registered custom <see cref="CustomActionButton"/>s.
    /// </summary>
    public static ReadOnlyCollection<CustomActionButton> Buttons { get; private set; } = new([]);

    /// <summary>
    /// Gets a list of all registered button-specific <see cref="MiraButtonClickEvent"/>s.
    /// </summary>
    public static ReadOnlyDictionary<Type, Type> EventTypes { get; private set; } = new(new Dictionary<Type, Type>());

    /// <summary>
    /// Gets a list of all registered button-specific <see cref="MiraButtonCancelledEvent"/>s.
    /// </summary>
    public static ReadOnlyDictionary<Type, Type> CancelledEventTypes { get; private set; } = new(new Dictionary<Type, Type>());

    internal static readonly List<CustomActionButton> CustomButtons = [];
    internal static readonly Dictionary<Type, Type> ButtonEventTypes = [];
    internal static readonly Dictionary<Type, Type> ButtonCancelledEventTypes = [];

    [SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Dynamic singleton initialization requires reflection to bypass the private field access modifier because the type is only known at runtime.")]
    internal static bool RegisterButton(in MiraPluginManager.RegistrationContext context)
    {
        var type = context.Type;
        var pluginInfo = context.PluginInfo;

        if (!type.IsAssignableTo(typeof(CustomActionButton)) || Activator.CreateInstance(type) is not CustomActionButton button)
        {
            return false;
        }

        CustomButtons.Add(button);
        pluginInfo.InternalButtons.Add(button);

        typeof(CustomButtonSingleton<>).MakeGenericType(type)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)! // Suppression message refers to this
            .SetValue(null, button);

        ButtonEventTypes.Add(type, typeof(MiraButtonClickEvent<>).MakeGenericType(type));
        ButtonCancelledEventTypes.Add(type, typeof(MiraButtonCancelledEvent<>).MakeGenericType(type));

        return true;
    }

    internal static void RegisterAllButtons()
    {
        // Save all buttons into a read-only collection for easy access
        Buttons = new ReadOnlyCollection<CustomActionButton>(CustomButtonManager.CustomButtons);
        EventTypes = new ReadOnlyDictionary<Type, Type>(CustomButtonManager.ButtonEventTypes);
        CancelledEventTypes = new ReadOnlyDictionary<Type, Type>(CustomButtonManager.ButtonCancelledEventTypes);
    }
}

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
    public static ReadOnlyCollection<CustomActionButton> Buttons { get; internal set; } = new([]);

    /// <summary>
    /// Gets a list of all registered button-specific <see cref="MiraButtonClickEvent"/>s.
    /// </summary>
    public static ReadOnlyDictionary<Type, Type> EventTypes { get; internal set; } = new(
        new Dictionary<Type, Type>
            { });

    /// <summary>
    /// Gets a list of all registered button-specific <see cref="MiraButtonCancelledEvent"/>s.
    /// </summary>
    public static ReadOnlyDictionary<Type, Type> CancelledEventTypes { get; internal set; } = new(
        new Dictionary<Type, Type>
            { });

    internal static readonly List<CustomActionButton> CustomButtons = [];
    internal static readonly Dictionary<Type, Type> ButtonEventTypes = [];
    internal static readonly Dictionary<Type, Type> ButtonCancelledEventTypes = [];

    [SuppressMessage(
        "Major Code Smell",
        "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields",
        Justification = "Dynamic singleton initialization requires reflection to bypass the private field access modifier because the type is only known at runtime."
    )]
    internal static bool RegisterButton(Type buttonType, MiraPluginInfo pluginInfo)
    {
        if (!buttonType.IsAssignableTo(typeof(CustomActionButton)) || Activator.CreateInstance(buttonType) is not CustomActionButton button)
        {
            return false;
        }

        CustomButtons.Add(button);
        pluginInfo.InternalButtons.Add(button);
        typeof(CustomButtonSingleton<>).MakeGenericType(buttonType)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)! // Suppression message refers to this
            .SetValue(null, button);

        ButtonEventTypes.Add(buttonType, typeof(MiraButtonClickEvent<>).MakeGenericType(buttonType));
        ButtonCancelledEventTypes.Add(buttonType, typeof(MiraButtonCancelledEvent<>).MakeGenericType(buttonType));

        return true;
    }
}

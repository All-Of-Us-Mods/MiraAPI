using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MiraAPI.Events.Mira;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

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

    internal static bool RegisterButton(Type type, in MiraPluginManager.RegistrationContext context)
    {
        if (!type.IsAssignableTo(typeof(CustomActionButton)) || Activator.CreateInstance(type) is not CustomActionButton button)
        {
            return false;
        }

        context.PluginInfo.InternalButtons.Add(button);

        CustomButtons.Add(button);
        ButtonEventTypes.Add(type, typeof(MiraButtonClickEvent<>).MakeGenericType(type));
        ButtonCancelledEventTypes.Add(type, typeof(MiraButtonCancelledEvent<>).MakeGenericType(type));

        Helpers.SetInstance(typeof(CustomButtonSingleton<>), type, button);

        return true;
    }

    internal static void RegisterAllButtons()
    {
        // Save all buttons into a read-only collection for easy access
        Buttons = new ReadOnlyCollection<CustomActionButton>(CustomButtons);
        EventTypes = new ReadOnlyDictionary<Type, Type>(ButtonEventTypes);
        CancelledEventTypes = new ReadOnlyDictionary<Type, Type>(ButtonCancelledEventTypes);
    }
}

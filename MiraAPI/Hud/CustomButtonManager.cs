using System;
using System.Collections.Generic;
using System.Reflection;
using Reactor.Utilities;

namespace MiraAPI.Hud;

public static class CustomButtonManager
{
    public static readonly List<CustomActionButton> CustomButtons = [];

    internal static void RegisterButton(Type buttonType)
    {
        if (!typeof(CustomActionButton).IsAssignableFrom(buttonType))
        {
            Logger<MiraApiPlugin>.Error($"{buttonType?.Name} does not inherit from CustomActionButton.");
            return;
        }

        if (Activator.CreateInstance(buttonType) is not CustomActionButton button)
        {
            Logger<MiraApiPlugin>.Error($"Failed to create button from {buttonType.Name}");
            return;
        }
        
        CustomButtons.Add(button);
        typeof(CustomButtonSingleton<>).MakeGenericType(buttonType)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, button);
    }
}
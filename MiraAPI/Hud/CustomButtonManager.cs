using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiraAPI.Hud;

public static class CustomButtonManager
{
    public static readonly List<CustomActionButton> CustomButtons = [];


    public static void RegisterButton(Type buttonType)
    {
        if (!typeof(CustomActionButton).IsAssignableFrom(buttonType))
        {
            return;
        }

        var button = (CustomActionButton)Activator.CreateInstance(buttonType);
        CustomButtons.Add(button);
        typeof(CustomButtonSingleton<>).MakeGenericType(buttonType)
            .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!
            .SetValue(null, button);
    }
}
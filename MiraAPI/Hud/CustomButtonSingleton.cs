using System;
using System.Linq;

namespace MiraAPI.Hud;

/// <summary>
/// A utility class to get the instance of a custom action button.
/// </summary>
/// <typeparam name="T">The type of the button you are trying to access.</typeparam>
public class CustomButtonSingleton<T> where T : CustomActionButton
{
    private static T? _instance;

    public static T Instance
    {
        get => _instance ??= CustomButtonManager.CustomButtons.OfType<T>().Single();
        set
        {
            if (_instance == value)
            {
                return;
            }

            if (_instance != null)
            {
                throw new InvalidOperationException($"Instance for {typeof(T)} is already set");
            }

            _instance = value;
        }
    }
}
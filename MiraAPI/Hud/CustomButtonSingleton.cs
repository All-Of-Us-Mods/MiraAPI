#nullable enable
using System;
using System.Linq;

namespace MiraAPI.Hud;

public class CustomButtonSingleton<T> where T : CustomActionButton
{
    private static T? _instance;

    public static T Instance
    {
        get => _instance ??= CustomButtonManager.CustomButtons.OfType<T>().Single();
        set
        {
            if (_instance == value) return;
            if (_instance != null) throw new InvalidOperationException($"Instance for {typeof(T)} is already set");
            _instance = value;
        }
    }
}
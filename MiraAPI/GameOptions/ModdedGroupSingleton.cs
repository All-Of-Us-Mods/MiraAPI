using System;
using System.Linq;

namespace MiraAPI.GameOptions;

public class ModdedGroupSingleton<T> where T : IModdedOptionGroup
{
    private static T _instance;

    public static T Instance
    {
        get => _instance ??= ModdedOptionsManager.Groups.Keys.OfType<T>().Single();
        set
        {
            if (_instance != null)
            {
                throw new InvalidOperationException($"Instance for {typeof(T)} is already set");
            }

            _instance = value;
        }
    }
}
using System;
using System.Linq;

namespace MiraAPI.Cosmetics;

public class CosmeticGroupSingleton<T> where T : AbstractCosmeticsGroup
{
    private static T _instance;

    public static T Instance
    {
        get => _instance ??= CustomCosmeticManager.Groups.OfType<T>().Single();
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
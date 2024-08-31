using System;
using System.Linq;

namespace MiraAPI.GameOptions;

/// <summary>
/// Singleton for option groups.
/// </summary>
/// <typeparam name="T">The option group type.</typeparam>
public class OptionGroupSingleton<T> where T : AbstractOptionGroup
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the option group.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can't</exception>
    public static T Instance => _instance ??= ModdedOptionsManager.Groups.OfType<T>().Single();
}

using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MiraAPI.GameOptions;

/// <summary>
/// Singleton for <see cref="AbstractOptionGroup"/>s.
/// </summary>
/// <typeparam name="T">The option group type.</typeparam>
public static class OptionGroupSingleton<T> where T : AbstractOptionGroup
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the <typeparamref name="T"/> group.
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is a utility class to get the instance of a custom option group.")]
    public static T Instance => _instance ??= ModdedOptionsManager.Groups.OfType<T>().Single();
}

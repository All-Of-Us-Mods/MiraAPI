using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Singleton for <see cref="LocalSettingsTab"/>s.
/// </summary>
/// <typeparam name="T">The settings tab type.</typeparam>
public static class LocalSettingsTabSingleton<T> where T : LocalSettingsTab
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the <typeparamref name="T"/> setting tab.
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is a utility class to get the instance of a local settings tab.")]
    public static T Instance => _instance ??= LocalSettingsManager.Tabs.OfType<T>().Single();
}

using System.Linq;

namespace MiraAPI.Hud;

/// <summary>
/// A utility class to get the instance of a custom action button.
/// </summary>
/// <typeparam name="T">The type of the button you are trying to access.</typeparam>
public static class CustomButtonSingleton<T> where T : CustomActionButton
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the button.
    /// </summary>
#pragma warning disable CA1000
    public static T Instance => _instance ??= CustomButtonManager.CustomButtons.OfType<T>().Single();
#pragma warning restore CA1000
}

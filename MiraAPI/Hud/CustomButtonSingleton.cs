using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MiraAPI.Hud;

/// <summary>
/// A utility class to get the instance of a <see cref="CustomActionButton"/>.
/// </summary>
/// <typeparam name="T">The type of the button you are trying to access.</typeparam>
public static class CustomButtonSingleton<T>
    where T : CustomActionButton
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the <typeparamref name="T"/> button.
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is a utility class to get the instance of a custom action button.")]
    public static T Instance => _instance ??= CustomButtonManager.CustomButtons.OfType<T>().Single();
}

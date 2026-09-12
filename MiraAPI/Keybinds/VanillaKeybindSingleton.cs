using System.Diagnostics.CodeAnalysis;

namespace MiraAPI.Keybinds;

/// <summary>
/// Singleton for getting a <see cref="VanillaKeybind"/> by <see cref="ActionButton"/>.
/// </summary>
/// <typeparam name="T"><see cref="ActionButton"/> type.</typeparam>
public static class VanillaKeybinding<T> where T : ActionButton
{
    [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types", Justification = "The instance holds a reference to the button that defines the class's type parameter.")]
    private static VanillaKeybind? _instance;

    /// <summary>
    /// Gets the instance of the <see cref="VanillaKeybind"/>.
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is a utility class to get the instance of a vanilla keybind.")]
    public static VanillaKeybind Instance => _instance ??= KeybindManager.VanillaKeybinds[typeof(T)];
}

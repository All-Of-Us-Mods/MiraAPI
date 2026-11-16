namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Provides unboxed access to option values to prevent memory allocations.
/// </summary>
/// <typeparam name="T">The type of the value that the option carries.</typeparam>
public interface ITypedOption<T>
{
    /// <summary>
    /// Gets the unboxed value of the option.
    /// </summary>
    /// <returns>The unboxed option value.</returns>
    T GetValueUnboxed();

    /// <summary>
    /// Sets the unboxed value of the option.
    /// </summary>
    /// <param name="value">The new value to set.</param>
    void SetValueUnboxed(T value);
}

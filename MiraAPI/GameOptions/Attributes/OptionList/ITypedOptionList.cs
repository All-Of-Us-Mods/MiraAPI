namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Provides unboxed access to option list values to prevent memory allocations.
/// </summary>
/// <typeparam name="T">The type of the value that the option list carries.</typeparam>
public interface ITypedOptionList<T>
{
    /// <summary>
    /// Gets the unboxed value of the option at the specified index.
    /// </summary>
    /// <param name="idx">The index of the option.</param>
    /// <returns>The option at the provided index.</returns>
    T GetValueUnboxed(int idx);

    /// <summary>
    /// Sets the unboxed value of the option at the specified index.
    /// </summary>
    /// <param name="idx">The index of the option.</param>
    /// <param name="value">The new value of the option to set.</param>
    void SetValueUnboxed(int idx, T value);
}

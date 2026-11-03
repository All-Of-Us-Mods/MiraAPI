using System;

namespace MiraAPI.Colors;

/// <summary>
/// Used to mark a class for <see cref="CustomColor"/> registration.
/// </summary>
/// <param name="idPrefix">The group translation ID prefix to attach to color names before registration.</param>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomColorsAttribute(string idPrefix) : Attribute
{
    /// <summary>
    /// Gets the translation ID prefix for all of the colors within the container type.
    /// </summary>
    public string IdPrefix { get; } = idPrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCustomColorsAttribute"/> class.
    /// </summary>
    public RegisterCustomColorsAttribute()
        : this(string.Empty)
    {
    }
}

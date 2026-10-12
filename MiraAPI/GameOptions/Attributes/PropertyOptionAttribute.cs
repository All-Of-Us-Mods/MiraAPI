using System;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Represents an attribute that is used to intercept a property's getter and setter.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class PropertyOptionAttribute : Attribute
{
    /// <summary>
    /// Sets the value of the option.
    /// </summary>
    /// <param name="value">The new value as an object.</param>
    public abstract void SetValue(object value);

    /// <summary>
    /// Gets the value of the option.
    /// </summary>
    /// <returns>The value of the option as an object.</returns>
    public abstract object GetValue();
}

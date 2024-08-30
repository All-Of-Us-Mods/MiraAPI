using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Represents an attribute that is used to define a modded option.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="roleType">Optional parameter to specify a role Type.</param>
[AttributeUsage(AttributeTargets.Property)]
public abstract class ModdedOptionAttribute(string title, Type? roleType = null) : Attribute
{
    /// <summary>
    /// Gets the IModdedOption object that this attribute is attached to.
    /// </summary>
    public IModdedOption? HolderOption { get; internal set; }

    /// <summary>
    /// Gets the title of the option.
    /// </summary>
    public string Title { get; private set; } = title;

    /// <summary>
    /// Gets the role type of the option.
    /// </summary>
    protected Type? RoleType { get; private set; } = roleType;

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

    internal abstract IModdedOption? CreateOption(object? value, PropertyInfo property);
}

using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Represents an attribute that is used to define an <see cref="IModdedOption"/>.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="roleType">Optional parameter to specify a role Type.</param>
/// <param name="modeType">Optional parameter to specify a game mode Type.</param>
[AttributeUsage(AttributeTargets.Property)]
public abstract class ModdedOptionAttribute(string? title = null, Type? roleType = null, Type? modeType = null) : PropertyOptionAttribute
{
    internal IModdedOption? HolderOption { get; set; }
    internal Type ValueType { get; set; }

    /// <summary>
    /// Gets the role type of the option.
    /// </summary>
    protected Type? RoleType { get; } = roleType;

    /// <summary>
    /// Gets the game mode type of the option.
    /// </summary>
    protected Type? ModeType { get; } = modeType;

    /// <summary>
    /// Gets or sets the title of the option.
    /// </summary>
    public string? Title { get; protected set; } = title;

    internal abstract IModdedOption? CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group);
}

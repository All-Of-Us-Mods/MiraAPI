using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Represents an attribute that is used to define an <see cref="IModdedOption"/>.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="roleType">Optional parameter to specify a role Type.</param>
/// <param name="modeType">Optional parameter to specify a game mode Type.</param>
[AttributeUsage(AttributeTargets.Property)]
[SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Internal behaviour that does not need property-level validation.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")] // why so many warnings???
public abstract class ModdedOptionAttribute(string? title = null, Type? roleType = null, Type? modeType = null) : PropertyOptionAttribute
{
    internal IModdedOption? HolderOption;
    internal Type ValueType;

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

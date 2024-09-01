using MiraAPI.Utilities;
using Reactor.Localization.Utilities;
using UnityEngine;

namespace MiraAPI.Colors;

/// <summary>
/// Represents a custom color with a main color and a shadow color.
/// </summary>
/// <param name="name">The name of the option.</param>
/// <param name="mainColor">The main color.</param>
/// <param name="shadowColor">The shadow color.</param>
public sealed class CustomColor(StringNames name, Color32 mainColor, Color32 shadowColor)
{
    /// <summary>
    /// Gets the main color.
    /// </summary>
    public Color32 MainColor { get; } = mainColor;

    /// <summary>
    /// Gets the shadow color.
    /// </summary>
    public Color32 ShadowColor { get; } = shadowColor;

    /// <summary>
    /// Gets the name of the color.
    /// </summary>
    public StringNames Name { get; } = name;

    /// <inheritdoc />
    public CustomColor(StringNames name, Color32 mainColor) : this(name, mainColor, mainColor.GetShadowColor(60))
    {
    }

    /// <inheritdoc />
    public CustomColor(string name, Color32 mainColor) : this(name, mainColor, mainColor.GetShadowColor(60))
    {
    }

    /// <inheritdoc />
    public CustomColor(string name, Color32 mainColor, Color32 shadowColor) : this(CustomStringName.CreateAndRegister(name), mainColor, shadowColor)
    {
    }
}

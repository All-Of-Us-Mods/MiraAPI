using MiraAPI.Translation;
using MiraAPI.Utilities;
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
    /// Gets or sets the main color.
    /// </summary>
    public Color32 MainColor { get; set; } = mainColor;

    /// <summary>
    /// Gets or sets the shadow color.
    /// </summary>
    public Color32 ShadowColor { get; set; } = shadowColor;

    /// <summary>
    /// Gets or sets the name of the color.
    /// </summary>
    public StringNames Name { get; set; } = name;

    /// <summary>
    /// Gets or sets whether the color is lighter or darker. Can be used to help with colorblind stuff.
    /// </summary>
    public CustomColorBrightness ColorBrightness { get; set; } = CustomColorBrightness.Darker;

    /// <inheritdoc />
    public CustomColor(StringNames name, Color32 mainColor) : this(name, mainColor, mainColor.GetShadowColor(60))
    {
    }

    /// <inheritdoc />
    public CustomColor(string name, Color32 mainColor) : this(name, mainColor, mainColor.GetShadowColor(60))
    {
    }

    /// <inheritdoc />
    public CustomColor(string name, Color32 mainColor, Color32 shadowColor) : this(MiraLocaleManager.GetOrCreateLocaleString(name), mainColor, shadowColor)
    {
    }
}

/// <summary>
/// Can be used to help with colorblind stuff.
/// </summary>
public enum CustomColorBrightness
{
    /// <summary>
    /// A lighter color.
    /// </summary>
    Lighter,

    /// <summary>
    /// A darker color.
    /// </summary>
    Darker,
}

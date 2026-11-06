using System;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Colors;

/// <summary>
/// Represents a custom color with a main color and a shadow color.
/// </summary>
/// <param name="nameId">The name of the option as a <see cref="StringNames"/> value.</param>
/// <param name="rawId">The name of the option as a <see cref="string"/> value.</param>
/// <param name="mainColor">The main color.</param>
/// <param name="shadowColor">The shadow color.</param>
/// <param name="brightness">The brightness type of the color.</param>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Internal code.")]
public class CustomColor(StringNames? nameId, string? rawId, Color32 mainColor, Color32 shadowColor, CustomColorBrightness brightness)
{
    private StringNames? name = nameId;
    private CustomColorBrightness colorBrightness = brightness;

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
    public StringNames Name
    {
        get
        {
            if (!name.HasValue && !string.IsNullOrEmpty(RawId))
                name = MiraLocaleManager.GetOrCreateLocaleString(RawId);

            return name.GetValueOrDefault();
        }
        internal set
        {
            name = value;
        }
    }

    /// <summary>
    /// Gets the id of the color.
    /// </summary>
    public int ColorId { get; internal set; } = -1;

    /// <summary>
    /// Gets or sets whether the color is lighter or darker.
    /// </summary>
    public CustomColorBrightness ColorBrightness
    {
        get => colorBrightness;
        [Obsolete("Use the constructor to set ColorBrightness instead. This will be removed in a future version.")]
        set => colorBrightness = value;
    }

    internal string? RawId = rawId;
    internal Color InternalMainColor = mainColor;
    internal Color InternalShadowColor = shadowColor;

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    public CustomColor(StringNames nameId, Color32 mainColor, Color32 shadowColor, CustomColorBrightness brightness)
        : this(nameId, null, mainColor, shadowColor, brightness)
    {
    }

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    public CustomColor(StringNames nameId, Color32 mainColor, CustomColorBrightness brightness)
        : this(nameId, null, mainColor, mainColor.DarkenAbsolute(60), brightness)
    {
    }

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    public CustomColor(string rawId, Color32 mainColor, Color32 shadowColor, CustomColorBrightness brightness)
        : this(null, rawId, mainColor, shadowColor, brightness)
    {
    }

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    public CustomColor(string rawId, Color32 mainColor, CustomColorBrightness brightness)
        : this(null, rawId, mainColor, mainColor.DarkenAbsolute(60), brightness)
    {
    }

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    [Obsolete("Use the constructor that takes in a CustomColorBrightness parameter. This will be removed in a future version.")]
    public CustomColor(StringNames nameId, Color32 mainColor, Color32 shadowColor)
        : this(nameId, null, mainColor, shadowColor, CustomColorBrightness.Darker)
    {
    }

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    [Obsolete("Use the constructor that takes in a CustomColorBrightness parameter. This will be removed in a future version.")]
    public CustomColor(StringNames nameId, Color32 mainColor)
        : this(nameId, null, mainColor, mainColor.DarkenAbsolute(60), CustomColorBrightness.Darker)
    {
    }

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    [Obsolete("Use the constructor that takes in a CustomColorBrightness parameter. This will be removed in a future version.")]
    public CustomColor(string rawId, Color32 mainColor, Color32 shadowColor)
        : this(null, rawId, mainColor, shadowColor, CustomColorBrightness.Darker)
    {
    }

    /// <inheritdoc cref="CustomColor(StringNames?, string?, Color32, Color32, CustomColorBrightness)"/>
    [Obsolete("Use the constructor that takes in a CustomColorBrightness parameter. This will be removed in a future version.")]
    public CustomColor(string rawId, Color32 mainColor)
        : this(null, rawId, mainColor, mainColor.DarkenAbsolute(60), CustomColorBrightness.Darker)
    {
    }
}

/// <summary>
/// Can be used to help with colorblind stuff.
/// </summary>
public enum CustomColorBrightness
{
    /// <summary>
    /// An invalid color.
    /// </summary>
    Invalid = -1,

    /// <summary>
    /// A lighter color.
    /// </summary>
    Lighter,

    /// <summary>
    /// A darker color.
    /// </summary>
    Darker,
}

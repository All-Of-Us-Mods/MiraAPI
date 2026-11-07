using System;
using MiraAPI.LocalSettings;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Colors;

/// <summary>
/// Represents a dynamic custom color that changes over time.
/// </summary>
/// <param name="nameId">The name of the option as a <see cref="StringNames"/> value.</param>
/// <param name="rawId">The name of the option as a <see cref="string"/> value.</param>
/// <param name="mainEvaluator">The function to evaluate the dynamic main color.</param>
/// <param name="shadowEvaluator">The function to evaluate the dynamic shadow color.</param>
/// <param name="mainColor">The static main color used when dynamic colors are disabled.</param>
/// <param name="shadowColor">The static shadow color used when dynamic colors are disabled.</param>
/// <param name="defaultTime">The default time that is used to automatically fill in the static colors when the latter is not provided.</param>
/// <param name="brightness">The brightness type of the color.</param>
public sealed class VariableColor(
    StringNames? nameId,
    string? rawId,
    Func<float, Color> mainEvaluator,
    Func<float, Color>? shadowEvaluator,
    Color32? mainColor,
    Color32? shadowColor,
    float defaultTime,
    CustomColorBrightness brightness)
    : CustomColor(
        nameId,
        rawId,
        mainColor ?? mainEvaluator(defaultTime),
        shadowColor ?? shadowEvaluator?.Invoke(defaultTime) ?? (mainColor ?? mainEvaluator(defaultTime)).DarkenAbsolute(60),
        brightness)
{
    /// <summary>
    /// Gets a delegate that takes in a time value and returns the dynamic main color.
    /// </summary>
    public Func<float, Color> MainEvaluator => mainEvaluator;

    /// <summary>
    /// Gets a delegate that takes in a time value and returns the dynamic shadow color.
    /// </summary>
    public Func<float, Color> ShadowEvaluator => shadowEvaluator ?? (t => mainEvaluator(t).DarkenAbsolute(0.24f));

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        StringNames nameId,
        Func<float, Color> mainEvaluator,
        Func<float, Color>? shadowEvaluator,
        CustomColorBrightness brightness,
        Color32? mainColor = null,
        Color32? shadowColor = null,
        float defaultTime = 0f)
        : this(nameId, null, mainEvaluator, shadowEvaluator, mainColor, shadowColor, defaultTime, brightness)
    {
    }

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        StringNames nameId,
        Func<float, Color> mainEvaluator,
        CustomColorBrightness brightness,
        Color32? mainColor = null,
        float defaultTime = 0f)
        : this(nameId, null, mainEvaluator, null, mainColor, null, defaultTime, brightness)
    {
    }

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        string rawId,
        Func<float, Color> mainEvaluator,
        Func<float, Color>? shadowEvaluator,
        CustomColorBrightness brightness,
        Color32? mainColor = null,
        Color32? shadowColor = null,
        float defaultTime = 0f)
        : this(null, rawId, mainEvaluator, shadowEvaluator, mainColor, shadowColor, defaultTime, brightness)
    {
    }

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        string rawId,
        Func<float, Color> mainEvaluator,
        CustomColorBrightness brightness,
        Color32? mainColor = null,
        float defaultTime = 0f)
        : this(null, rawId, mainEvaluator, null, mainColor, null, defaultTime, brightness)
    {
    }

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        StringNames nameId,
        Func<float, Color> mainEvaluator,
        Func<float, Color>? shadowEvaluator,
        Color32? mainColor = null,
        Color32? shadowColor = null,
        float defaultTime = 0f)
        : this(nameId, null, mainEvaluator, shadowEvaluator, mainColor, shadowColor, defaultTime, CustomColorBrightness.Darker)
    {
    }

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        StringNames nameId,
        Func<float, Color> mainEvaluator,
        Color32? mainColor = null,
        float defaultTime = 0f)
        : this(nameId, null, mainEvaluator, null, mainColor, null, defaultTime, CustomColorBrightness.Darker)
    {
    }

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        string rawId,
        Func<float, Color> mainEvaluator,
        Func<float, Color>? shadowEvaluator,
        Color32? mainColor = null,
        Color32? shadowColor = null,
        float defaultTime = 0f)
        : this(null, rawId, mainEvaluator, shadowEvaluator, mainColor, shadowColor, defaultTime, CustomColorBrightness.Darker)
    {
    }

    /// <inheritdoc cref="VariableColor(StringNames?, string?, Func{float, Color}, Func{float, Color}?, Color32?, Color32?, float, CustomColorBrightness)"/>
    public VariableColor(
        string rawId,
        Func<float, Color> mainEvaluator,
        Color32? mainColor = null,
        float defaultTime = 0f)
        : this(null, rawId, mainEvaluator, null, mainColor, null, defaultTime, CustomColorBrightness.Darker)
    {
    }

    /// <summary>
    /// Evaluates the dynamic main color based on the provided time value.
    /// </summary>
    /// <param name="time">The time to evaluate the color against.</param>
    /// <returns>The dynamically evaluated color, or the static <see cref="CustomColor.MainColor"/> if static variable colors are enabled in the user's settings.</returns>
    public Color EvaluateMainColor(float time)
    {
        return LocalSettingsTabSingleton<MiraApiSettings>.Instance.EnableVariableColors.Value
            ? MainEvaluator(time)
            : InternalMainColor;
    }

    /// <summary>
    /// Evaluates the dynamic shadow color based on the provided time value.
    /// </summary>
    /// <param name="time">The time to evaluate the color against.</param>
    /// <returns>The dynamically evaluated color, or the static <see cref="CustomColor.ShadowColor"/> if static variable colors are enabled in the user's settings.</returns>
    public Color EvaluateShadowColor(float time)
    {
        return LocalSettingsTabSingleton<MiraApiSettings>.Instance.EnableVariableColors.Value
            ? ShadowEvaluator(time)
            : InternalShadowColor;
    }
}

using System;
using MiraAPI.LocalSettings;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Colors;

/// <summary>
/// Represents a dynamic custom color that changes over time.
/// </summary>
/// <param name="name">The name of the color.</param>
/// <param name="mainEvaluator">The function to evaluate the dynamic main color.</param>
/// <param name="shadowEvaluator">The function to evaluate the dynamic shadow color.</param>
/// <param name="mainColor">The static main color used when dynamic colors are disabled.</param>
/// <param name="shadowColor">The static shadow color used when dynamic colors are disabled.</param>
public sealed class VariableColor(StringNames name, Func<float, Color> mainEvaluator, Func<float, Color>? shadowEvaluator, Color32? mainColor = null, Color32? shadowColor = null)
    : CustomColor(name, mainColor ?? mainEvaluator(0f), shadowColor ?? shadowEvaluator?.Invoke(0f) ?? (mainColor ?? mainEvaluator(0f)).GetShadowColor(60))
{
    /// <summary>
    /// Gets or sets a delegate that takes in a time value and returns the dynamic main color.
    /// </summary>
    public Func<float, Color> MainEvaluator { get; set; } = mainEvaluator;

    /// <summary>
    /// Gets or sets a delegate that takes in a time value and returns the dynamic shadow color.
    /// </summary>
    public Func<float, Color> ShadowEvaluator { get; set; } = shadowEvaluator ?? (t => mainEvaluator(t).GetShadowColor(0.24f));

    /// <inheritdoc cref="VariableColor"/>
    public VariableColor(StringNames name, Func<float, Color> mainEvaluator, Color32? mainColor = null)
        : this(name, mainEvaluator, null, mainColor, null)
    {
    }

    /// <inheritdoc cref="VariableColor"/>
    public VariableColor(string name, Func<float, Color> mainEvaluator, Color32? mainColor = null)
        : this(name, mainEvaluator, null, mainColor, null)
    {
    }

    /// <inheritdoc cref="VariableColor"/>
    public VariableColor(string name, Func<float, Color> mainEvaluator, Func<float, Color>? shadowEvaluator, Color32? mainColor = null, Color32? shadowColor = null)
        : this(MiraLocaleManager.GetOrCreateLocaleString(name), mainEvaluator, shadowEvaluator, mainColor, shadowColor)
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

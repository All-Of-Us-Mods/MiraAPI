using UnityEngine;

namespace MiraAPI.Utilities;

/// <summary>
/// Extension methods for the <see cref="Color"/> struct.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Gets the relative luminance of a <see cref="Color"/> (WCAG 2.1 specification). 0 is black, 1 is white.
    /// </summary>
    /// <param name="color"><see cref="Color"/> to calculate luminance for.</param>
    /// <returns>Luminance value between 0 and 1.</returns>
    public static float GetRelativeLuminance(this Color color)
    {
        return (0.2126f * Linearize(color.r)) +
               (0.7152f * Linearize(color.g)) +
               (0.0722f * Linearize(color.b));
    }

    /// <summary>
    /// Linearizes an sRGB component into linear space.
    /// </summary>
    /// <param name="c">The component to linearize.</param>
    /// <returns>The linearized result.</returns>
    public static float Linearize(float c)
    {
        return c <= 0.03928f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    /// <summary>
    /// Calculates the contrast ratio between two <see cref="Color"/>s (WCAG 2.1 specification).
    /// </summary>
    /// <param name="colorA">First <see cref="Color"/>.</param>
    /// <param name="colorB">Second <see cref="Color"/>.</param>
    /// <returns>Contrast ratio between the two <see cref="Color"/>s.</returns>
    public static float GetContrastRatio(this Color colorA, Color colorB)
    {
        var luminanceA = colorA.GetRelativeLuminance();
        var luminanceB = colorB.GetRelativeLuminance();
        return (Mathf.Max(luminanceA, luminanceB) + 0.05f) / (Mathf.Min(luminanceA, luminanceB) + 0.05f);
    }

    /// <summary>
    /// Generates an accessible alternate <see cref="Color"/> with sufficient contrast.
    /// </summary>
    /// <param name="color">Base <see cref="Color"/>.</param>
    /// <param name="desiredRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>Alternate <see cref="Color"/> with sufficient contrast.</returns>
    public static Color FindAlternateColor(this Color color, float desiredRatio = 4.5f)
    {
        var lightColor = FindMinimumContrastColor(color, Color.white, desiredRatio);
        var darkColor = FindMinimumContrastColor(color, Color.black, desiredRatio);

        return lightColor.GetContrastRatio(color) >= darkColor.GetContrastRatio(color) ? lightColor : darkColor;
    }

    /// <summary>
    /// Finds the <see cref="Color"/> with the minimum contrast ratio to the target <see cref="Color"/>.
    /// </summary>
    /// <param name="baseColor">The base <see cref="Color"/> to start from.</param>
    /// <param name="targetColor">The target <see cref="Color"/> to compare against.</param>
    /// <param name="desiredRatio">The desired contrast ratio.</param>
    /// <returns>The <see cref="Color"/> with the minimum contrast ratio to the target <see cref="Color"/>.</returns>
    public static Color FindMinimumContrastColor(Color baseColor, Color targetColor, float desiredRatio)
    {
        var baseLuminance = baseColor.GetRelativeLuminance();
        var isLightening = targetColor == Color.white;

        var targetLuminance = isLightening
            ? desiredRatio * (baseLuminance + 0.05f) - 0.05f
            : (baseLuminance + 0.05f) / desiredRatio - 0.05f;

        if (isLightening && targetLuminance > 1f) return Color.white;
        if (!isLightening && targetLuminance < 0f) return Color.black;

        float low = 0f, high = 1f;
        var bestColor = targetColor;
        for (var i = 0; i < 10; i++)
        {
            var t = (low + high) / 2f;
            var testColor = Color.Lerp(baseColor, targetColor, t);
            var testLuminance = testColor.GetRelativeLuminance();

            if ((isLightening && testLuminance >= targetLuminance) || (!isLightening && testLuminance <= targetLuminance))
            {
                high = t;
                bestColor = testColor;
            }
            else
            {
                low = t;
            }
        }
        return bestColor;
    }
}

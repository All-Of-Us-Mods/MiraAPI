using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MiraAPI.Utilities;

/// <summary>
/// Extension methods for the <see cref="Color"/> and <see cref="Color32"/> structs.
/// </summary>
public static class ColorExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ToFloat(this byte @byte)
    {
        return @byte / 255f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ToByte(this float @float)
    {
        return (byte)MathF.Round(@float * 255f);
    }

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
    /// Gets the relative luminance of a <see cref="Color32"/> (WCAG 2.1 specification). 0 is black, 1 is white.
    /// </summary>
    /// <param name="color"><see cref="Color32"/> to calculate luminance for.</param>
    /// <returns>Luminance value between 0 and 1.</returns>
    public static float GetRelativeLuminance(this Color32 color)
    {
        return (0.2126f * Linearize(color.r.ToFloat())) +
               (0.7152f * Linearize(color.g.ToFloat())) +
               (0.0722f * Linearize(color.b.ToFloat()));
    }

    /// <summary>
    /// Linearizes an sRGB component into linear space.
    /// </summary>
    /// <param name="c">The component to linearize.</param>
    /// <returns>The linearized result.</returns>
    public static float Linearize(float c)
    {
        return c <= 0.03928f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
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

        return (Math.Max(luminanceA, luminanceB) + 0.05f) / (Math.Min(luminanceA, luminanceB) + 0.05f);
    }

    /// <summary>
    /// Calculates the contrast ratio between two <see cref="Color32"/>s.
    /// </summary>
    /// <param name="colorA">First <see cref="Color32"/>.</param>
    /// <param name="colorB">Second <see cref="Color32"/>.</param>
    /// <returns>Contrast ratio between the two <see cref="Color32"/>s.</returns>
    public static float GetContrastRatio(this Color32 colorA, Color32 colorB)
    {
        var luminanceA = colorA.GetRelativeLuminance();
        var luminanceB = colorB.GetRelativeLuminance();

        return (Math.Max(luminanceA, luminanceB) + 0.05f) / (Math.Min(luminanceA, luminanceB) + 0.05f);
    }

    /// <summary>
    /// Generates an accessible alternate <see cref="Color"/> by shifting towards black or white
    /// based on which extreme offers the highest potential contrast from the original color.
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
    /// Generates an accessible alternate <see cref="Color32"/> with sufficient contrast.
    /// </summary>
    /// <param name="color">Base <see cref="Color32"/>.</param>
    /// <param name="desiredRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>Alternate <see cref="Color32"/> with sufficient contrast.</returns>
    public static Color32 FindAlternateColor(this Color32 color, float desiredRatio = 4.5f)
    {
        var colorF = new Color(color.r.ToFloat(), color.g.ToFloat(), color.b.ToFloat());
        var result = colorF.FindAlternateColor(desiredRatio);
        return new Color32(result.r.ToByte(), result.g.ToByte(), result.b.ToByte(), color.a);
    }

    /// <summary>
    /// Darkens a <see cref="Color"/> towards black until it reaches the target WCAG contrast ratio against a background.
    /// </summary>
    /// <param name="color">The foreground <see cref="Color"/> to modify.</param>
    /// <param name="background">The background <see cref="Color"/> it sits on.</param>
    /// <param name="targetRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>A darkened <see cref="Color"/> meeting the contrast ratio, or black if impossible.</returns>
    public static Color DarkenToWCAG(this Color color, Color background, float targetRatio = 4.5f)
    {
        return color.GetContrastRatio(background) >= targetRatio
            ? color
            : FindMinimumContrastColor(color, Color.black, targetRatio);
    }

    /// <summary>
    /// Darkens a <see cref="Color32"/> towards black until it reaches the target WCAG contrast ratio against a background.
    /// </summary>
    /// <param name="color">The foreground <see cref="Color32"/> to modify.</param>
    /// <param name="background">The background <see cref="Color32"/> it sits on.</param>
    /// <param name="targetRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>A darkened <see cref="Color32"/> meeting the contrast ratio, or black if impossible.</returns>
    public static Color32 DarkenToWCAG(this Color32 color, Color32 background, float targetRatio = 4.5f)
    {
        var colorF = new Color(color.r.ToFloat(), color.g.ToFloat(), color.b.ToFloat(), color.a.ToFloat());
        var bgF = new Color(background.r.ToFloat(), background.g.ToFloat(), background.b.ToFloat(), background.a.ToFloat());

        var result = colorF.DarkenToWCAG(bgF, targetRatio);
        return new Color32(result.r.ToByte(), result.g.ToByte(), result.b.ToByte(), color.a);
    }

    /// <summary>
    /// Lightens a <see cref="Color"/> towards white until it reaches the target WCAG contrast ratio against a background.
    /// </summary>
    /// <param name="color">The foreground <see cref="Color"/> to modify.</param>
    /// <param name="background">The background <see cref="Color"/> it sits on.</param>
    /// <param name="targetRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>A lightened <see cref="Color"/> meeting the contrast ratio, or white if impossible.</returns>
    public static Color LightenToWCAG(this Color color, Color background, float targetRatio = 4.5f)
    {
        return color.GetContrastRatio(background) >= targetRatio
            ? color
            : FindMinimumContrastColor(color, Color.white, targetRatio);
    }

    /// <summary>
    /// Lightens a <see cref="Color32"/> towards white until it reaches the target WCAG contrast ratio against a background.
    /// </summary>
    /// <param name="color">The foreground <see cref="Color32"/> to modify.</param>
    /// <param name="background">The background <see cref="Color32"/> it sits on.</param>
    /// <param name="targetRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>A lightened <see cref="Color32"/> meeting the contrast ratio, or white if impossible.</returns>
    public static Color32 LightenToWCAG(this Color32 color, Color32 background, float targetRatio = 4.5f)
    {
        var colorF = new Color(color.r.ToFloat(), color.g.ToFloat(), color.b.ToFloat(), color.a.ToFloat());
        var bgF = new Color(background.r.ToFloat(), background.g.ToFloat(), background.b.ToFloat(), background.a.ToFloat());

        var result = colorF.LightenToWCAG(bgF, targetRatio);
        return new Color32(result.r.ToByte(), result.g.ToByte(), result.b.ToByte(), color.a);
    }

    /// <summary>
    /// Automatically decides whether to lighten or darken the <see cref="Color"/> to achieve WCAG compliance
    /// based on which extreme offers the highest potential contrast against the provided background.
    /// </summary>
    /// <param name="color">The foreground <see cref="Color"/> to modify.</param>
    /// <param name="background">The background <see cref="Color"/> it sits on.</param>
    /// <param name="targetRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>A modified <see cref="Color"/> meeting the WCAG contrast requirement.</returns>
    public static Color EnsureWCAGContrast(this Color color, Color background, float targetRatio = 4.5f)
    {
        if (color.GetContrastRatio(background) >= targetRatio)
            return color;

        var blackContrast = Color.black.GetContrastRatio(background);
        var whiteContrast = Color.white.GetContrastRatio(background);

        return blackContrast > whiteContrast
            ? color.DarkenToWCAG(background, targetRatio)
            : color.LightenToWCAG(background, targetRatio);
    }

    /// <summary>
    /// Automatically decides whether to lighten or darken the <see cref="Color32"/> to achieve WCAG compliance.
    /// </summary>
    /// <param name="color">The foreground <see cref="Color32"/> to modify.</param>
    /// <param name="background">The background <see cref="Color32"/> it sits on.</param>
    /// <param name="targetRatio">Target contrast ratio (default 4.5f for AA).</param>
    /// <returns>A modified <see cref="Color32"/> meeting the WCAG contrast requirement.</returns>
    public static Color32 EnsureWCAGContrast(this Color32 color, Color32 background, float targetRatio = 4.5f)
    {
        var colorF = new Color(color.r.ToFloat(), color.g.ToFloat(), color.b.ToFloat(), color.a.ToFloat());
        var bgF = new Color(background.r.ToFloat(), background.g.ToFloat(), background.b.ToFloat(), background.a.ToFloat());

        var result = colorF.EnsureWCAGContrast(bgF, targetRatio);
        return new Color32(result.r.ToByte(), result.g.ToByte(), result.b.ToByte(), color.a);
    }

    /// <summary>
    /// Finds the <see cref="Color"/> with the minimum contrast ratio to the target <see cref="Color"/> using binary search.
    /// </summary>
    /// <param name="baseColor">The base <see cref="Color"/> to start from.</param>
    /// <param name="targetColor">The target <see cref="Color"/> to approach (usually white or black).</param>
    /// <param name="desiredRatio">The desired contrast ratio.</param>
    /// <returns>The <see cref="Color"/> with the minimum contrast ratio to the target <see cref="Color"/>.</returns>
    public static Color FindMinimumContrastColor(Color baseColor, Color targetColor, float desiredRatio)
    {
        var baseLuminance = baseColor.GetRelativeLuminance();
        var isLightening = targetColor == Color.white;

        var targetLuminance = isLightening
            ? desiredRatio * (baseLuminance + 0.05f) - 0.05f
            : (baseLuminance + 0.05f) / desiredRatio - 0.05f;

        if (isLightening && targetLuminance > 1f)
            return Color.white;

        if (!isLightening && targetLuminance < 0f)
            return Color.black;

        float low = 0f, high = 1f;
        var bestColor = targetColor;

        for (var i = 0; i < 10; i++)
        {
            var t = (low + high) / 2f;

            var testColor = new Color(
                baseColor.r + (targetColor.r - baseColor.r) * t,
                baseColor.g + (targetColor.g - baseColor.g) * t,
                baseColor.b + (targetColor.b - baseColor.b) * t,
                baseColor.a);

            var testLuminance = testColor.GetRelativeLuminance();

            if (isLightening ? testLuminance >= targetLuminance : testLuminance <= targetLuminance)
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

    /// <summary>
    /// Subtracts a flat amount from the <see cref="Color"/>'s RGB channels.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="amount">The flat amount to subtract (typically 0.0 to 1.0).</param>
    /// <returns>A uniformly darkened <see cref="Color"/>.</returns>
    public static Color DarkenAbsolute(this Color color, float amount)
    {
        return new Color(
            Math.Max(0f, color.r - amount),
            Math.Max(0f, color.g - amount),
            Math.Max(0f, color.b - amount),
            color.a);
    }

    /// <summary>
    /// Subtracts a flat byte amount from the <see cref="Color32"/>'s RGB channels.
    /// </summary>
    /// <param name="color">The original <see cref="Color32"/>.</param>
    /// <param name="amount">The flat amount to subtract (0 to 255).</param>
    /// <returns>A uniformly darkened <see cref="Color32"/>.</returns>
    public static Color32 DarkenAbsolute(this Color32 color, byte amount)
    {
        return new Color32(
            (byte)Math.Max(0, color.r - amount),
            (byte)Math.Max(0, color.g - amount),
            (byte)Math.Max(0, color.b - amount),
            color.a);
    }

    /// <summary>
    /// Adds a flat amount to the <see cref="Color"/>'s RGB channels.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="amount">The flat amount to add (typically 0.0 to 1.0).</param>
    /// <returns>A uniformly lightened <see cref="Color"/>.</returns>
    public static Color LightenAbsolute(this Color color, float amount)
    {
        return new Color(
            Math.Min(1f, color.r + amount),
            Math.Min(1f, color.g + amount),
            Math.Min(1f, color.b + amount),
            color.a);
    }

    /// <summary>
    /// Adds a flat byte amount to the <see cref="Color32"/>'s RGB channels.
    /// </summary>
    /// <param name="color">The original <see cref="Color32"/>.</param>
    /// <param name="amount">The flat amount to add (0 to 255).</param>
    /// <returns>A uniformly lightened <see cref="Color32"/>.</returns>
    public static Color32 LightenAbsolute(this Color32 color, byte amount)
    {
        return new Color32(
            (byte)Math.Min(255, color.r + amount),
            (byte)Math.Min(255, color.g + amount),
            (byte)Math.Min(255, color.b + amount),
            color.a);
    }

    /// <summary>
    /// Darkens the <see cref="Color"/> multiplicatively, preserving the ratio between RGB channels to maintain hue.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="percentage">The percentage to darken by (e.g., 0.2 for 20% darker).</param>
    /// <returns>A ratio-preserved darkened <see cref="Color"/>.</returns>
    public static Color DarkenRelative(this Color color, float percentage)
    {
        var multiplier = Math.Max(0f, Math.Min(1f, 1f - percentage));
        return new Color(
            color.r * multiplier,
            color.g * multiplier,
            color.b * multiplier,
            color.a);
    }

    /// <summary>
    /// Darkens the <see cref="Color32"/> multiplicatively, preserving the ratio between RGB channels.
    /// </summary>
    /// <param name="color">The original <see cref="Color32"/>.</param>
    /// <param name="percentage">The percentage to darken by (e.g., 0.2 for 20% darker).</param>
    /// <returns>A ratio-preserved darkened <see cref="Color32"/>.</returns>
    public static Color32 DarkenRelative(this Color32 color, float percentage)
    {
        var multiplier = Math.Max(0f, Math.Min(1f, 1f - percentage));
        return new Color32(
            (color.r * multiplier).ToByte(),
            (color.g * multiplier).ToByte(),
            (color.b * multiplier).ToByte(),
            color.a);
    }

    /// <summary>
    /// Lightens the <see cref="Color"/> multiplicatively, preserving the ratio between RGB channels.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="percentage">The percentage to lighten by (e.g., 0.2 for 20% lighter).</param>
    /// <returns>A ratio-preserved lightened <see cref="Color"/>.</returns>
    public static Color LightenRelative(this Color color, float percentage)
    {
        var multiplier = 1f + percentage;
        return new Color(
            Math.Min(1f, color.r * multiplier),
            Math.Min(1f, color.g * multiplier),
            Math.Min(1f, color.b * multiplier),
            color.a);
    }

    /// <summary>
    /// Lightens the <see cref="Color32"/> multiplicatively, preserving the ratio between RGB channels.
    /// </summary>
    /// <param name="color">The original <see cref="Color32"/>.</param>
    /// <param name="percentage">The percentage to lighten by (e.g., 0.2 for 20% lighter).</param>
    /// <returns>A ratio-preserved lightened <see cref="Color32"/>.</returns>
    public static Color32 LightenRelative(this Color32 color, float percentage)
    {
        var multiplier = 1f + percentage;
        return new Color32(
            Math.Min(255f, color.r * 255f * multiplier / 255f).ToByte(),
            Math.Min(255f, color.g * 255f * multiplier / 255f).ToByte(),
            Math.Min(255f, color.b * 255f * multiplier / 255f).ToByte(),
            color.a);
    }
}

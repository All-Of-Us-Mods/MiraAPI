using System;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Colors.ColorSpaces;

/// <summary>
/// A struct that denotes the HSB (Hue, Saturation, Brightness) color space.
/// </summary>
/// <param name="h">The hue component.</param>
/// <param name="s">The saturation component.</param>
/// <param name="b">The brightness component.</param>
/// <param name="a">The alpha component.</param>
[Serializable]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Unity parity.")]
public record struct HsbColor(float h, float s, float b, float a = 1f)
{
    /// <summary>
    /// Implicitly converts an <see cref="HsbColor"/> to a Unity <see cref="Color"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator Color(in HsbColor c)
    {
        return ToColor(c);
    }

    /// <summary>
    /// Implicitly converts a Unity <see cref="Color"/> to an <see cref="HsbColor"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator HsbColor(in Color c)
    {
        return FromColor(c);
    }

    /// <summary>
    /// Converts an <see cref="HsbColor"/> to a standard Unity RGB <see cref="Color"/>.
    /// </summary>
    /// <param name="hsb">The HSB color to convert.</param>
    /// <returns>A Unity <see cref="Color"/> representing the same visual color.</returns>
    public static Color ToColor(in HsbColor hsb)
    {
        if (hsb.s <= float.Epsilon)
            return new Color(MathUtilities.Clamp01(hsb.b), MathUtilities.Clamp01(hsb.b), MathUtilities.Clamp01(hsb.b), hsb.a);

        var c = hsb.b * hsb.s;
        var hPrime = hsb.h * 6f;
        var x = c * (1f - MathF.Abs((hPrime % 2f) - 1f));
        var m = hsb.b - c;

        var (r, g, b) = hPrime switch
        {
            < 1f => (c, x, 0f),
            < 2f => (x, c, 0f),
            < 3f => (0f, c, x),
            < 4f => (0f, x, c),
            < 5f => (x, 0f, c),
            <= 6f => (c, 0f, x),
            _ => (0f, 0f, 0f),
        };

        return new Color(
            MathUtilities.Clamp01(r + m),
            MathUtilities.Clamp01(g + m),
            MathUtilities.Clamp01(b + m),
            hsb.a);
    }

    /// <summary>
    /// Converts a standard Unity RGB <see cref="Color"/> to an <see cref="HsbColor"/>.
    /// </summary>
    /// <param name="color">The Unity Color to convert.</param>
    /// <returns>An <see cref="HsbColor"/> representing the same visual color.</returns>
    public static HsbColor FromColor(in Color color)
    {
        var max = MathF.Max(color.r, MathF.Max(color.g, color.b));
        var min = MathF.Min(color.r, MathF.Min(color.g, color.b));
        var delta = max - min;

        var h = 0f;

        if (delta > 0f)
        {
            if (max == color.r)
                h = (color.g - color.b) / delta % 6f;
            else if (max == color.g)
                h = (color.b - color.r) / delta + 2f;
            else
                h = (color.r - color.g) / delta + 4f;

            h /= 6f;

            if (h < 0f)
                h += 1f;
        }

        var s = max == 0f ? 0f : delta / max;
        var b = max;

        return new HsbColor(h, s, b, color.a);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HsbColor"/> values, using the shortest path for the hue.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors, clamped to a 0 to 1 range.</param>
    /// <returns>The interpolated <see cref="HsbColor"/>.</returns>
    public static HsbColor Lerp(in HsbColor a, in HsbColor b, float t)
    {
        return LerpUnclamped(a, b, MathUtilities.Clamp01(t));
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HsbColor"/> values without clamping the interpolant.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors.</param>
    /// <returns>The interpolated <see cref="HsbColor"/>.</returns>
    public static HsbColor LerpUnclamped(in HsbColor a, in HsbColor b, float t)
    {
        var hueDifference = MathUtilities.Repeat(b.h - a.h, 1f);

        if (hueDifference > 0.5f)
            hueDifference -= 1f;

        return new HsbColor(
            MathUtilities.Repeat(a.h + (hueDifference * t), 1f),
            MathUtilities.LerpUnclamped(a.s, b.s, t),
            MathUtilities.LerpUnclamped(a.b, b.b, t),
            MathUtilities.LerpUnclamped(a.a, b.a, t));
    }
}

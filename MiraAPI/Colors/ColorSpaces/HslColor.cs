using System;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Colors.ColorSpaces;

/// <summary>
/// A struct that denotes the HSL (Hue, Saturation, Lightness) color space.
/// </summary>
/// <param name="h">The hue component.</param>
/// <param name="s">The saturation component.</param>
/// <param name="l">The lightness component.</param>
/// <param name="a">The alpha component.</param>
[Serializable]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Unity parity.")]
public record struct HslColor(float h, float s, float l, float a = 1f)
{
    /// <summary>
    /// Implicitly converts an <see cref="HslColor"/> to a Unity <see cref="Color"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator Color(in HslColor c)
    {
        return ToColor(c);
    }

    /// <summary>
    /// Implicitly converts a Unity <see cref="Color"/> to an <see cref="HslColor"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator HslColor(in Color c)
    {
        return FromColor(c);
    }

    /// <summary>
    /// Converts an <see cref="HslColor"/> to a standard Unity RGB <see cref="Color"/>.
    /// </summary>
    /// <param name="hsl">The HSL color to convert.</param>
    /// <returns>A Unity <see cref="Color"/> representing the same visual color.</returns>
    public static Color ToColor(in HslColor hsl)
    {
        if (hsl.s <= float.Epsilon)
            return new Color(MathUtilities.Clamp01(hsl.l), MathUtilities.Clamp01(hsl.l), MathUtilities.Clamp01(hsl.l), hsl.a);

        var c = (1f - MathF.Abs(2f * hsl.l - 1f)) * hsl.s;
        var hPrime = hsl.h * 6f;
        var x = c * (1f - MathF.Abs((hPrime % 2f) - 1f));
        var m = hsl.l - c / 2f;

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

        return new Color(MathUtilities.Clamp01(r + m), MathUtilities.Clamp01(g + m), MathUtilities.Clamp01(b + m), hsl.a);
    }

    /// <summary>
    /// Converts a standard Unity RGB <see cref="Color"/> to an <see cref="HslColor"/>.
    /// </summary>
    /// <param name="color">The Unity Color to convert.</param>
    /// <returns>An <see cref="HslColor"/> representing the same visual color.</returns>
    public static HslColor FromColor(in Color color)
    {
        var max = MathF.Max(color.r, MathF.Max(color.g, color.b));
        var min = MathF.Min(color.r, MathF.Min(color.g, color.b));
        var l = (max + min) / 2f;
        var delta = max - min;

        var s = delta == 0f ? 0f : delta / (1f - MathF.Abs(2f * l - 1f));

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

        return new HslColor(h, s, l, color.a);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HslColor"/> values, using the shortest path for the hue.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors, clamped to a 0 to 1 range.</param>
    /// <returns>The interpolated <see cref="HslColor"/>.</returns>
    public static HslColor Lerp(in HslColor a, in HslColor b, float t)
    {
        return LerpUnclamped(a, b, MathUtilities.Clamp01(t));
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HslColor"/> values without clamping the interpolant.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors.</param>
    /// <returns>The interpolated <see cref="HslColor"/>.</returns>
    public static HslColor LerpUnclamped(in HslColor a, in HslColor b, float t)
    {
        var hueDifference = MathUtilities.Repeat(b.h - a.h, 1f);

        if (hueDifference > 0.5f)
            hueDifference -= 1f;

        return new HslColor(
            MathUtilities.Repeat(a.h + (hueDifference * t), 1f),
            MathUtilities.LerpUnclamped(a.s, b.s, t),
            MathUtilities.LerpUnclamped(a.l, b.l, t),
            MathUtilities.LerpUnclamped(a.a, b.a, t));
    }
}

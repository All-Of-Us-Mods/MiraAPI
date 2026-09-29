using System;
using System.Diagnostics.CodeAnalysis;
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
    public static implicit operator Color(HslColor c)
    {
        return ToColor(c);
    }

    /// <summary>
    /// Implicitly converts a Unity <see cref="Color"/> to an <see cref="HslColor"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator HslColor(Color c)
    {
        return FromColor(c);
    }

    /// <summary>
    /// Converts an <see cref="HslColor"/> to a standard Unity RGB <see cref="Color"/>.
    /// </summary>
    /// <param name="hsl">The HSL color to convert.</param>
    /// <returns>A Unity <see cref="Color"/> representing the same visual color.</returns>
    public static Color ToColor(HslColor hsl)
    {
        if (hsl.s <= Mathf.Epsilon)
            return new Color(Mathf.Clamp01(hsl.l), Mathf.Clamp01(hsl.l), Mathf.Clamp01(hsl.l), hsl.a);

        var c = (1f - Mathf.Abs(2f * hsl.l - 1f)) * hsl.s;
        var hPrime = hsl.h * 6f;
        var x = c * (1f - Mathf.Abs((hPrime % 2f) - 1f));
        var m = hsl.l - c / 2f;

        var (r, g, b) = hPrime switch
        {
            < 1f => (c, x, 0f),
            < 2f => (x, c, 0f),
            < 3f => (0f, c, x),
            < 4f => (0f, x, c),
            < 5f => (x, 0f, c),
            < 6f => (c, 0f, x),
            _ => (0f, 0f, 0f),
        };

        return new Color(Mathf.Clamp01(r + m), Mathf.Clamp01(g + m), Mathf.Clamp01(b + m), hsl.a);
    }

    /// <summary>
    /// Converts a standard Unity RGB <see cref="Color"/> to an <see cref="HslColor"/>.
    /// </summary>
    /// <param name="color">The Unity Color to convert.</param>
    /// <returns>An <see cref="HslColor"/> representing the same visual color.</returns>
    public static HslColor FromColor(Color color)
    {
        var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        var min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        var l = (max + min) / 2f;
        var delta = max - min;

        var s = delta == 0f ? 0f : delta / (1f - Mathf.Abs(2f * l - 1f));

        Color.RGBToHSV(color, out var h, out _, out _);

        return new HslColor(h, s, l, color.a);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HslColor"/> values, using the shortest path for the hue.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors, clamped to a 0 to 1 range.</param>
    /// <returns>The interpolated <see cref="HslColor"/>.</returns>
    public static HslColor Lerp(HslColor a, HslColor b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HslColor"/> values without clamping the interpolant.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors.</param>
    /// <returns>The interpolated <see cref="HslColor"/>.</returns>
    public static HslColor LerpUnclamped(HslColor a, HslColor b, float t)
    {
        var hueDifference = Mathf.Repeat(b.h - a.h, 1f);

        if (hueDifference > 0.5f)
            hueDifference -= 1f;

        return new HslColor(
            Mathf.Repeat(a.h + (hueDifference * t), 1f),
            Mathf.LerpUnclamped(a.s, b.s, t),
            Mathf.LerpUnclamped(a.l, b.l, t),
            Mathf.LerpUnclamped(a.a, b.a, t));
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
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
    public static implicit operator Color(HsbColor c)
    {
        return ToColor(c);
    }

    /// <summary>
    /// Implicitly converts a Unity <see cref="Color"/> to an <see cref="HsbColor"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator HsbColor(Color c)
    {
        return FromColor(c);
    }

    /// <summary>
    /// Converts an <see cref="HsbColor"/> to a standard Unity RGB <see cref="Color"/>.
    /// </summary>
    /// <param name="hsb">The HSB color to convert.</param>
    /// <returns>A Unity <see cref="Color"/> representing the same visual color.</returns>
    public static Color ToColor(HsbColor hsb)
    {
        var r = hsb.b;
        var g = hsb.b;
        var b = hsb.b;

        if (hsb.s <= Mathf.Epsilon)
            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsb.a);

        var max = hsb.b;
        var dif = hsb.b * hsb.s;
        var min = hsb.b - dif;

        var h = hsb.h * 360f;

        (r, g, b) = h switch
        {
            < 60f => (max, h * dif / 60f + min, min),
            < 120f => (-(h - 120f) * dif / 60f + min, max, min),
            < 180f => (min, max, (h - 120f) * dif / 60f + min),
            < 240f => (min, -(h - 240f) * dif / 60f + min, max),
            < 300f => ((h - 240f) * dif / 60f + min, min, max),
            <= 360f => (max, min, -(h - 360f) * dif / 60f + min),
            _ => (0f, 0f, 0f),
        };

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsb.a);
    }

    /// <summary>
    /// Converts a standard Unity RGB <see cref="Color"/> to an <see cref="HsbColor"/>.
    /// </summary>
    /// <param name="color">The Unity Color to convert.</param>
    /// <returns>An <see cref="HsbColor"/> representing the same visual color.</returns>
    public static HsbColor FromColor(Color color)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        return new HsbColor(h, s, v, color.a);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HsbColor"/> values, using the shortest path for the hue.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors, clamped to a 0 to 1 range.</param>
    /// <returns>The interpolated <see cref="HsbColor"/>.</returns>
    public static HsbColor Lerp(HsbColor a, HsbColor b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="HsbColor"/> values without clamping the interpolant.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors.</param>
    /// <returns>The interpolated <see cref="HsbColor"/>.</returns>
    public static HsbColor LerpUnclamped(HsbColor a, HsbColor b, float t)
    {
        var hueDifference = Mathf.Repeat(b.h - a.h, 1f);

        if (hueDifference > 0.5f)
            hueDifference -= 1f;

        return new HsbColor(
            Mathf.Repeat(a.h + (hueDifference * t), 1f),
            Mathf.LerpUnclamped(a.s, b.s, t),
            Mathf.LerpUnclamped(a.b, b.b, t),
            Mathf.LerpUnclamped(a.a, b.a, t));
    }
}

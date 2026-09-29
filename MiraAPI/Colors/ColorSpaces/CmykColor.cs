using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace MiraAPI.Colors.ColorSpaces;

/// <summary>
/// A struct that denotes the CMYK (Cyan, Magenta, Yellow, Key) color space.
/// </summary>
/// <param name="c">The cyan component.</param>
/// <param name="m">The magenta component.</param>
/// <param name="y">The yellow component.</param>
/// <param name="k">The key component.</param>
/// <param name="a">The alpha component.</param>
[Serializable]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Unity parity.")]
public record struct CmykColor(float c, float m, float y, float k, float a = 1f)
{
    /// <summary>
    /// Implicitly converts a <see cref="CmykColor"/> to a Unity <see cref="Color"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator Color(CmykColor c)
    {
        return ToColor(c);
    }

    /// <summary>
    /// Implicitly converts a Unity <see cref="Color"/> to a <see cref="CmykColor"/>.
    /// </summary>
    /// <param name="c">The color being converted.</param>
    public static implicit operator CmykColor(Color c)
    {
        return FromColor(c);
    }

    /// <summary>
    /// Converts a <see cref="CmykColor"/> to a standard Unity RGB <see cref="Color"/>.
    /// </summary>
    /// <param name="cmyk">The CMYK color to convert.</param>
    /// <returns>A Unity <see cref="Color"/> representing the same visual color.</returns>
    public static Color ToColor(CmykColor cmyk)
    {
        var r = (1f - cmyk.c) * (1f - cmyk.k);
        var g = (1f - cmyk.m) * (1f - cmyk.k);
        var b = (1f - cmyk.y) * (1f - cmyk.k);

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), cmyk.a);
    }

    /// <summary>
    /// Converts a standard Unity RGB <see cref="Color"/> to a <see cref="CmykColor"/>.
    /// </summary>
    /// <param name="color">The Unity Color to convert.</param>
    /// <returns>A <see cref="CmykColor"/> representing the same visual color.</returns>
    public static CmykColor FromColor(Color color)
    {
        var r = color.r;
        var g = color.g;
        var b = color.b;

        var k = 1f - Mathf.Max(r, g, b);

        if (k >= 1f)
            return new CmykColor(0f, 0f, 0f, 1f, color.a);

        var c = (1f - r - k) / (1f - k);
        var m = (1f - g - k) / (1f - k);
        var y = (1f - b - k) / (1f - k);

        return new CmykColor(Mathf.Clamp01(c), Mathf.Clamp01(m), Mathf.Clamp01(y), Mathf.Clamp01(k), color.a);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="CmykColor"/> values.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors, clamped to a 0 to 1 range.</param>
    /// <returns>The interpolated <see cref="CmykColor"/>.</returns>
    public static CmykColor Lerp(CmykColor a, CmykColor b, float t)
    {
        t = Mathf.Clamp01(t);
        return LerpUnclamped(a, b, t);
    }

    /// <summary>
    /// Linearly interpolates between two <see cref="CmykColor"/> values without clamping the interpolant.
    /// </summary>
    /// <param name="a">The starting color.</param>
    /// <param name="b">The ending color.</param>
    /// <param name="t">The interpolation value between the two colors.</param>
    /// <returns>The interpolated <see cref="CmykColor"/>.</returns>
    public static CmykColor LerpUnclamped(CmykColor a, CmykColor b, float t)
    {
        return new CmykColor(
            Mathf.LerpUnclamped(a.c, b.c, t),
            Mathf.LerpUnclamped(a.m, b.m, t),
            Mathf.LerpUnclamped(a.y, b.y, t),
            Mathf.LerpUnclamped(a.k, b.k, t),
            Mathf.LerpUnclamped(a.a, b.a, t));
    }
}

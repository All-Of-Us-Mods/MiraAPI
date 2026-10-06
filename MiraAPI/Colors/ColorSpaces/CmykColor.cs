using System;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.Utilities;
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

        return new Color(MathUtilities.Clamp01(r), MathUtilities.Clamp01(g), MathUtilities.Clamp01(b), cmyk.a);
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

        var k = 1f - MathF.Max(r, MathF.Max(g, b));

        if (k >= 1f)
            return new CmykColor(0f, 0f, 0f, 1f, color.a);

        var invK = 1f / (1f - k);

        var c = (1f - r - k) * invK;
        var m = (1f - g - k) * invK;
        var y = (1f - b - k) * invK;

        return new CmykColor(MathUtilities.Clamp01(c), MathUtilities.Clamp01(m), MathUtilities.Clamp01(y), MathUtilities.Clamp01(k), color.a);
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
        return LerpUnclamped(a, b, MathUtilities.Clamp01(t));
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
            MathUtilities.LerpUnclamped(a.c, b.c, t),
            MathUtilities.LerpUnclamped(a.m, b.m, t),
            MathUtilities.LerpUnclamped(a.y, b.y, t),
            MathUtilities.LerpUnclamped(a.k, b.k, t),
            MathUtilities.LerpUnclamped(a.a, b.a, t));
    }
}

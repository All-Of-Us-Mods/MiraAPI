using System;

namespace MiraAPI.Utilities;

/// <summary>
/// Provides mathematical utility methods.
/// </summary>
public static class MathUtilities
{
    /// <summary>
    /// Linearly interpolates between two values, clamping the interpolant between 0 and 1.
    /// </summary>
    /// <param name="a">The starting value.</param>
    /// <param name="b">The ending value.</param>
    /// <param name="t">The interpolation value, automatically clamped to a 0 to 1 range.</param>
    /// <returns>The interpolated float.</returns>
    public static float Lerp(float a, float b, float t)
    {
        return a + ((b - a) * Clamp01(t));
    }

    /// <summary>
    /// Linearly interpolates between two values without clamping the interpolant.
    /// </summary>
    /// <param name="a">The starting value.</param>
    /// <param name="b">The ending value.</param>
    /// <param name="t">The interpolation value.</param>
    /// <returns>The interpolated float.</returns>
    public static float LerpUnclamped(float a, float b, float t)
    {
        return a + ((b - a) * t);
    }

    /// <summary>
    /// Clamps a floating-point value strictly between 0 and 1.
    /// </summary>
    /// <param name="value">The floating-point value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp01(float value)
    {
        return Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Ping-pongs a value back and forth between a minimum and maximum value.
    /// </summary>
    /// <param name="t">The input time or interpolant.</param>
    /// <param name="min">The minimum boundary of the ping-pong wave.</param>
    /// <param name="max">The maximum boundary of the ping-pong wave.</param>
    /// <param name="speed">The multiplier for the speed of the ping-pong effect.</param>
    /// <returns>A value oscillating continuously between <paramref name="min"/> and <paramref name="max"/>.</returns>
    public static float PingPong(float t, float min, float max, float speed = 1f)
    {
        return min + PingPong(t, max - min, speed);
    }

    /// <summary>
    /// Ping-pongs a value back and forth between 0 and a specified length.
    /// </summary>
    /// <param name="t">The input time or interpolant.</param>
    /// <param name="length">The maximum value to bounce against, starting from 0.</param>
    /// <param name="speed">The multiplier for the speed of the ping-pong effect.</param>
    /// <returns>A value oscillating continuously between 0 and <paramref name="length"/>.</returns>
    public static float PingPong(float t, float length, float speed = 1f)
    {
        var dx = speed * t;
        var floor = (int)MathF.Floor(dx);
        var phase = floor % 2; // 0 = forward, 1 = backward
        return Clamp01(((dx - floor) * ((2 * phase) - 1)) + 1 - phase) * length;
    }

    /// <summary>
    /// Loops a value strictly between 0 and a maximum length, behaving like a standard float modulo.
    /// </summary>
    /// <param name="t">The input value to loop.</param>
    /// <param name="length">The maximum length of the repeating sequence.</param>
    /// <returns>The wrapped value smoothly looping back to 0 upon reaching <paramref name="length"/>.</returns>
    public static float Repeat(float t, float length)
    {
        return Math.Clamp(t - (MathF.Floor(t / length) * length), 0f, length);
    }
}

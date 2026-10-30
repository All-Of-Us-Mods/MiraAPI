using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MiraAPI.Colors;

/// <summary>
/// Used to register and track <see cref="CustomColor"/>s.
/// </summary>
public static class PaletteManager
{
    /// <summary>
    /// Gets all registered <see cref="CustomColor"/>s.
    /// </summary>
    public static CustomColor[] RegisteredColors => [.. CustomColors];

    internal static readonly List<CustomColor> CustomColors = [];
    internal static readonly HashSet<StringNames> VariableColorNames = [];

    private static readonly CustomColorBrightness[] ColorBrightnessMap =
    [
        CustomColorBrightness.Darker,  // Red
        CustomColorBrightness.Darker,  // Blue
        CustomColorBrightness.Darker,  // Green
        CustomColorBrightness.Lighter, // Pink
        CustomColorBrightness.Lighter, // Orange
        CustomColorBrightness.Lighter, // Yellow
        CustomColorBrightness.Darker,  // Black
        CustomColorBrightness.Lighter, // White
        CustomColorBrightness.Darker,  // Purple
        CustomColorBrightness.Darker,  // Brown
        CustomColorBrightness.Lighter, // Cyan
        CustomColorBrightness.Lighter, // Lime
        CustomColorBrightness.Darker,  // Maroon
        CustomColorBrightness.Lighter, // Rose
        CustomColorBrightness.Lighter, // Banana
        CustomColorBrightness.Darker,  // Gray
        CustomColorBrightness.Darker,  // Tan
        CustomColorBrightness.Lighter, // Coral
    ];

    private static int originalLength;

    internal static void RegisterAllColors()
    {
        originalLength = Palette.PlayerColors.Length;

        var colors = CustomColors.Select(x => x.MainColor).ToArray();
        var shadowColors = CustomColors.Select(x => x.ShadowColor).ToArray();
        var stringNames = CustomColors.Select(x => x.Name).ToArray();
        var newColorId = originalLength;
        foreach (var color in CustomColors)
        {
            color.ColorId = newColorId++;
        }

        Palette.PlayerColors = Palette.PlayerColors.ToArray().AddRangeToArray(colors);
        Palette.ShadowColors = Palette.ShadowColors.ToArray().AddRangeToArray(shadowColors);
        Palette.ColorNames = Palette.ColorNames.ToArray().AddRangeToArray(stringNames);

        Palette.TextColors = Palette.TextColors.ToArray().AddRangeToArray(colors);
        Palette.TextOutlineColors = Palette.TextOutlineColors.ToArray().AddRangeToArray(shadowColors);
    }

    /// <summary>
    /// Checks to see if the provided color ID belongs to a variable color.
    /// </summary>
    /// <param name="colorId">The id of the color.</param>
    /// <returns><c>true</c> if the color is a variable color; <c>false</c> otherwise.</returns>
    public static bool IsVariable(int colorId)
    {
        if ((uint)colorId >= (uint)Palette.ColorNames.Length)
            return false;

        return VariableColorNames.Contains(Palette.ColorNames[colorId]);
    }

    /// <summary>
    /// Checks to see if the provided color ID belongs to a custom color.
    /// </summary>
    /// <param name="colorId">The ID of the color.</param>
    /// <returns><c>true</c> if the color is a custom color; <c>false</c> otherwise.</returns>
    public static bool IsCustomColor(int colorId)
    {
        return (uint)(colorId - originalLength) < (uint)CustomColors.Count;
    }

    /// <summary>
    /// Gets the main color of the provided color ID.
    /// </summary>
    /// <param name="colorId">The id of the color.</param>
    /// <returns>The main color associated with the provided ID.</returns>
    public static Color GetMainColor(int colorId)
    {
        if ((uint)colorId >= (uint)Palette.ColorNames.Length)
            return Color.white;

        return !TryGetCustomColor(colorId, out var custom)
            ? Palette.PlayerColors[colorId]
            : (custom is VariableColor variable
                ? variable.EvaluateMainColor(Time.time)
                : custom.InternalMainColor);
    }

    /// <summary>
    /// Gets the shadow color of the provided color ID.
    /// </summary>
    /// <param name="colorId">The ID of the color.</param>
    /// <returns>The shadow color associated with the provided ID.</returns>
    public static Color GetShadowColor(int colorId)
    {
        if ((uint)colorId >= (uint)Palette.ColorNames.Length)
            return Color.white;

        return !TryGetCustomColor(colorId, out var custom)
            ? Palette.ShadowColors[colorId]
            : (custom is VariableColor variable
                ? variable.EvaluateShadowColor(Time.time)
                : custom.InternalShadowColor);
    }

    /// <summary>
    /// Gets the brightness type of the color associated with the provided ID.
    /// </summary>
    /// <param name="colorId">The ID of the color.</param>
    /// <returns>The brightness type of the provided ID.</returns>
    public static CustomColorBrightness GetBrightness(int colorId)
    {
        if ((uint)colorId >= (uint)Palette.ColorNames.Length)
            return CustomColorBrightness.Invalid;

        return TryGetCustomColor(colorId, out var custom)
            ? custom.ColorBrightness
            : ColorBrightnessMap[colorId];
    }

    internal static bool TryGetCustomColor(int colorId, [NotNullWhen(true)] out CustomColor? custom)
    {
        var index = colorId - originalLength;

        if ((uint)index >= (uint)CustomColors.Count)
        {
            custom = null;
            return false;
        }

        custom = CustomColors[index];
        return true;
    }
}

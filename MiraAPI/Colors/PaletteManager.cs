using System.Collections.Generic;
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
    internal static readonly Dictionary<int, CustomColor> ColorIdToColorMap = [];

    internal static void RegisterAllColors()
    {
        var colors = CustomColors.Select(x => x.MainColor).ToArray();
        var shadowColors = CustomColors.Select(x => x.ShadowColor).ToArray();
        var stringNames = CustomColors.Select(x => x.Name).ToArray();

        var originalLength = Palette.PlayerColors.Length;

        Palette.PlayerColors = Palette.PlayerColors.ToArray().AddRangeToArray(colors);
        Palette.ShadowColors = Palette.ShadowColors.ToArray().AddRangeToArray(shadowColors);
        Palette.ColorNames = Palette.ColorNames.ToArray().AddRangeToArray(stringNames);

        Palette.TextColors = Palette.TextColors.ToArray().AddRangeToArray(colors);
        Palette.TextOutlineColors = Palette.TextOutlineColors.ToArray().AddRangeToArray(shadowColors);

        for (var i = 0; i < originalLength + CustomColors.Count; i++)
            ColorIdToColorMap[i + originalLength] = CustomColors[i];
    }

    /// <summary>
    /// Checks to see if the provided color ID belongs to a variant color.
    /// </summary>
    /// <param name="colorId">The id of the color.</param>
    /// <returns><c>true</c> if the color is a variable color; <c>false</c> otherwise.</returns>
    public static bool IsVariable(int colorId)
    {
        return VariableColorNames.Contains(Palette.ColorNames[colorId]);
    }

    /// <summary>
    /// Checks to see if the provided color ID belongs to a custom color.
    /// </summary>
    /// <param name="colorId">The id of the color.</param>
    /// <returns><c>true</c> if the color is a custom color; <c>false</c> otherwise.</returns>
    public static bool IsCustomColor(int colorId)
    {
        return ColorIdToColorMap.ContainsKey(colorId);
    }

    /// <summary>
    /// Gets the main color of the provided color ID.
    /// </summary>
    /// <param name="colorId">The id of the color.</param>
    /// <returns>The main color associated with the provided ID.</returns>
    public static Color32 GetMainColor(int colorId)
    {
        return !ColorIdToColorMap.TryGetValue(colorId, out var custom)
            ? Palette.PlayerColors[colorId]
            : (custom is VariableColor variable
                ? variable.EvaluateMainColor()
                : custom.MainColor);
    }

    /// <summary>
    /// Gets the shadow color of the provided color ID.
    /// </summary>
    /// <param name="colorId">The id of the color.</param>
    /// <returns>The shadow color associated with the provided ID.</returns>
    public static Color32 GetShadowColor(int colorId)
    {
        return !ColorIdToColorMap.TryGetValue(colorId, out var custom)
            ? Palette.ShadowColors[colorId]
            : (custom is VariableColor variable
                ? variable.EvaluateShadowColor()
                : custom.ShadowColor);
    }
}

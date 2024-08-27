using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace MiraAPI.Utilities.Colors;

internal static class PaletteManager
{
    public static readonly List<CustomColor> CustomColors = [];
    
    public static void RegisterAllColors()
    {
        var colors = CustomColors.Select(x => x.MainColor);
        var shadowColors = CustomColors.Select(x => x.ShadowColor);
        var stringNames = CustomColors.Select(x => x.Name);

        Palette.PlayerColors = Palette.PlayerColors.ToArray().AddRangeToArray(colors.ToArray());
        Palette.ShadowColors = Palette.ShadowColors.ToArray().AddRangeToArray(shadowColors.ToArray());
        Palette.ColorNames = Palette.ColorNames.ToArray().AddRangeToArray(stringNames.ToArray());
    } 
}
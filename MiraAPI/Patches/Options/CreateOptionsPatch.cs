using HarmonyLib;
using MiraAPI.GameOptions;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
public static class CreateOptionsPatch
{
    public static void Postfix()
    {
        foreach (var (_, value) in ModdedOptionsManager.ModdedOptions)
        {
            value.ResetToConfig();
        }
    }
}

using HarmonyLib;
using GameModesEnum = AmongUs.GameOptions.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.SetGameMode))]
internal static class CreateOptionsPatch
{
    public static bool Prefix(CreateOptionsPicker __instance, GameModesEnum mode)
    {
        if (mode <= GameModesEnum.SeekFools)
        {
            return true;
        }

        // TODO: finish this
        return false;
    }
}

using HarmonyLib;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.SetGameMode))]
internal static class CreateOptionsPatch
{
    public static bool Prefix(CreateOptionsPicker __instance, AmongUs.GameOptions.GameModes mode)
    {
        if (mode <= AmongUs.GameOptions.GameModes.SeekFools)
        {
            return true;
        }

        // TODO: finish this
        return false;
    }
}
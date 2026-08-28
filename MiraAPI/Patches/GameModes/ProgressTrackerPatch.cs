using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;
[HarmonyPatch(typeof(ProgressTracker), nameof(ProgressTracker.Start))]
internal static class ProgressTrackerPatch
{
    public static bool Prefix(ProgressTracker __instance)
    {
        if (CustomGameModeManager.ActiveMode != null && !CustomGameModeManager.ActiveMode.ShowTaskBar)
        {
            __instance.gameObject.SetActive(false);
            return false;
        }
        return true;
    }
}

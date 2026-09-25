using HarmonyLib;

namespace MiraAPI.Patches;

/// <summary>
/// Used to fix the <see cref="Minigame"/> crash errors (credits Submerged/PH Gaming for the fix).
/// </summary>
[HarmonyPatch]
public static class MinigameBeginErrorPatch
{
    private static readonly Logger Logger = new(Logger.Category.Gameplay, "Minigame");

    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [HarmonyPrefix]
    public static void Prefix(Minigame __instance)
    {
        __instance.logger ??= Logger;
    }
}

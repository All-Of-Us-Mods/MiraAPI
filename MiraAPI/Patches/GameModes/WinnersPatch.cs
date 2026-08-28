using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal static class WinnersPatch
{
    [HarmonyPostfix]
    public static void ShowWinningScreen()
    {
        if (CustomGameModeManager.ActiveMode is null or ClassicMode)
            return;

        var winners = CustomGameModeManager.ActiveMode.CalculateWinners();

        if (winners is null)
            return;

        EndGameResult.CachedWinners.Clear();

        foreach (var data in winners)
        {
            EndGameResult.CachedWinners.Add(new(data));
        }
    }
}

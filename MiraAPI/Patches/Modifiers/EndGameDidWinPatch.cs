using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Patches.Modifiers;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
internal static class EndGameDidWinPatch
{
    private static List<CachedPlayerData> CachedWinners { get; set; } = [];

    public static void Prefix(EndGameResult endGameResult)
    {
        CachedWinners.Clear();
        var gameOverReason = endGameResult.GameOverReason;

        var players = GameData.Instance.AllPlayers.ToArray();
        for (var i = 0; i < GameData.Instance.PlayerCount; i++)
        {
            var networkedPlayerInfo = players[i];
            if (!networkedPlayerInfo)
            {
                continue;
            }

            var didWin = networkedPlayerInfo.Role.DidWin(gameOverReason);

            if (!networkedPlayerInfo.Object)
            {
                if (didWin)
                {
                    CachedWinners.Add(new CachedPlayerData(networkedPlayerInfo));
                }
                continue;
            }

            foreach (var modifier in networkedPlayerInfo.Object.GetModifiers<GameModifier>().OrderByDescending(x => x.Priority()))
            {
                var result = modifier.DidWin(gameOverReason);
                if (!result.HasValue) continue;

                didWin = result.Value;
                break;
            }

            if (didWin)
            {
                CachedWinners.Add(new CachedPlayerData(networkedPlayerInfo));
            }
        }
    }

    public static void Postfix()
    {
        EndGameResult.CachedWinners.Clear();
        foreach (var winner in CachedWinners)
        {
            EndGameResult.CachedWinners.Add(winner);
        }
    }
}

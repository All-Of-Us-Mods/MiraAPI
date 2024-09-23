using System.Linq;
using HarmonyLib;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Modifiers;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoEndGame))]
public static class EndGameDidWinPatch
{
    public static void Prefix()
    {
        var gameOverReason = EndGameResult.CachedGameOverReason;
        EndGameResult.CachedWinners.Clear();

        var players = GameData.Instance.AllPlayers.ToArray();
        for (var i = 0; i < GameData.Instance.PlayerCount; i++)
        {
            var networkedPlayerInfo = players[i];
            if (networkedPlayerInfo == null)
            {
                continue;
            }

            var didWin = networkedPlayerInfo.Role.DidWin(gameOverReason);

            var modifierWin = networkedPlayerInfo.Object?.GetModifierComponent()?.ActiveModifiers.OfType<GameModifier>()
                .FirstOrDefault(x => x.DidWin(gameOverReason) != null)?.DidWin(gameOverReason);
            if (modifierWin != null)
            {
                didWin = modifierWin.Value;
            }

            if (didWin)
            {
                EndGameResult.CachedWinners.Add(new CachedPlayerData(networkedPlayerInfo));
            }
        }
    }
}

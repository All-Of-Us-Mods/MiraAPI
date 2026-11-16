using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;

namespace MiraAPI.Patches.Events;

internal static class EndGamePatches
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
    public static class GameManagerRpcEndGamePatch
    {
        public static bool Prefix(GameOverReason endReason)
        {
            var @event = new BeforeGameEndEvent(endReason);
            @event.InvokeEvent();

            return !@event.IsCancelled;
        }
    }

    [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
    public static class EndGameManagerPatches
    {
        public static bool Prefix(EndGameManager __instance)
        {
            if (CustomGameOver.Instance is not { } gameOver)
            {
                return true;
            }

            var result = gameOver.BeforeEndGameSetup(__instance);
            return result;
        }

        public static void Postfix(EndGameManager __instance)
        {
            if (CustomGameOver.Instance is { } gameOver)
            {
                gameOver.AfterEndGameSetup(__instance);
                CustomGameOver.Instance = null;
            }

            var @event = new GameEndEvent(__instance);
            @event.InvokeEvent();
        }
    }
}

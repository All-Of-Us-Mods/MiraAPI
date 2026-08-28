using HarmonyLib;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch]
internal static class GameLogicPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(GameManager), nameof(GameManager.DidHumansWin))]
    public static bool DidHumansWin(GameOverReason reason, ref bool __result)
    {
        __result = reason == GameOverReason.CrewmatesByTask || reason == GameOverReason.CrewmatesByVote ||
                   reason == GameOverReason.ImpostorDisconnect || reason == GameOverReason.HideAndSeek_CrewmatesByTimer;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GameManager), nameof(GameManager.DidImpostorsWin))]
    public static bool DidImpostorsWin(GameOverReason reason, ref bool __result)
    {
        __result = reason == GameOverReason.ImpostorsByKill || reason == GameOverReason.ImpostorsBySabotage ||
                   reason == GameOverReason.ImpostorsByVote || reason == GameOverReason.HideAndSeek_ImpostorsByKills ||
                   reason == GameOverReason.CrewmateDisconnect;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetBodyType))]
    public static bool BodyTypePatch(NormalGameManager __instance, PlayerControl player, ref PlayerBodyTypes __result)
    {
        if (CustomGameModeManager.ActiveMode == null || !CustomGameModeManager.ActiveMode.GameModeBodyTypeOverride)
        {
            return true;
        }
        __result = CustomGameModeManager.ActiveMode.GetBodyType(player);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static bool EndGamePatch(LogicGameFlowNormal __instance)
    {
        if (TutorialManager.InstanceExists)
        {
            return true;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return false;
        }

        if (!GameData.Instance)
        {
            return false;
        }

        if (CustomGameModeManager.ActiveMode != null)
        {
            CustomGameModeManager.ActiveMode.CheckGameEnd(out var runOriginal, __instance);

            if (!runOriginal)
            {
                return false;
            }
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    public static void IsGameOverDueToDeathPatch(LogicGameFlowNormal __instance, ref bool __result)
    {
        if (CustomGameModeManager.IsActiveGameMode<HideAndSeekMode>())
        {
            __result = false;
        }
    }
}

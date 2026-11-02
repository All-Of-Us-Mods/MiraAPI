using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch(typeof(GameManager))]
internal static class GameManagerPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(GameManager.OnPlayerDeath))]
    public static bool OnDeathPrefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] bool assignGhostRole)
    {
        if (CustomGameModeManager.ActiveMode != null)
        {
            CustomGameModeManager.ActiveMode.OnPlayerDeath(player, assignGhostRole);
            return false;
        }

        return true;
    }

    [HarmonyPostfix, HarmonyPatch(nameof(GameManager.StartGame))]
    public static void StartGamePostfix()
    {
        if (CustomGameModeManager.ActiveMode != null && HudManager.InstanceExists)
        {
            HudManager.Instance.CrewmatesKilled.gameObject.SetActive(CustomGameModeManager.ActiveMode is HideAndSeekMode);
        }
    }

    [HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetMapOptions))]
    [HarmonyPrefix]
    public static bool GetMapOptions(ref MapOptions __result)
    {
        if (CustomGameModeManager.ActiveMode == null)
        {
            return true;
        }

        __result = CustomGameModeManager.ActiveMode.GetMapOptions();
        return false;
    }
}

[HarmonyPatch(typeof(GameStartManager))]
internal static class GameStartManagerPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(GameStartManager.Start))]
    public static void StartPatch(GameStartManager __instance)
    {
        if (MiraApiPlugin.IsDevBuild)
        {
            __instance.MinPlayers = 1;
        }
    }

    [HarmonyPrefix, HarmonyPatch(nameof(GameStartManager.BeginGame))]
    public static void Prefix(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        ModdedOptionsManager.SyncAllOptions();
        CustomRoleManager.SyncAllRoleSettings();
        GameModeOption.RpcSyncGamemode(PlayerControl.LocalPlayer, GameModeOption.Value);
    }
}

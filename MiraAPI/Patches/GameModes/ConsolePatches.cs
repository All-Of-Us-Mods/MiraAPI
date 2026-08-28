using HarmonyLib;
using InnerNet;
using MiraAPI.GameModes;

namespace MiraAPI.Patches.GameModes;

[HarmonyPatch]
internal static class ConsolePatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    public static bool CanUsePatch(Console __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && ShipStatus.Instance)
        {
            if (CustomGameModeManager.ActiveMode == null || CustomGameModeManager.ActiveMode.CanUseTasks(__instance))
            {
                var task = __instance.FindTask(pc.Object);

                if (task && task.GetComponent<SabotageTask>())
                {
                    canUse = couldUse = true;
                    return true;
                }

                canUse = false;
                couldUse = false;
                return true;
            }

            canUse = couldUse = false;
            return false;
        }

        canUse = couldUse = true;
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
    public static bool SystemCanUsePatch(SystemConsole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && ShipStatus.Instance)
        {
            if (CustomGameModeManager.ActiveMode == null || CustomGameModeManager.ActiveMode.CanUseSystemConsole(__instance))
            {
                canUse = couldUse = true;
                return true;
            }

            canUse = couldUse = false;
            return false;
        }

        canUse = couldUse = true;
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MapConsole), nameof(MapConsole.CanUse))]
    public static bool MapCanUsePatch(MapConsole __instance, [HarmonyArgument(0)] NetworkedPlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
    {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && ShipStatus.Instance)
        {
            if (CustomGameModeManager.ActiveMode == null || CustomGameModeManager.ActiveMode.CanUseMapConsole(__instance))
            {
                canUse = couldUse = true;
                return true;
            }

            canUse = couldUse = false;
            return false;
        }

        canUse = couldUse = true;
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MapConsole), nameof(MapConsole.Use))]
    public static bool MapUsePatch(MapConsole __instance)
    {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && ShipStatus.Instance)
        {
            return CustomGameModeManager.ActiveMode == null ||
                   CustomGameModeManager.ActiveMode.CanUseMapConsole(__instance);
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
    public static bool SystemUsePatch(SystemConsole __instance)
    {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && ShipStatus.Instance)
        {
            return CustomGameModeManager.ActiveMode == null ||
                   CustomGameModeManager.ActiveMode.CanUseSystemConsole(__instance);
        }

        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Console), nameof(Console.Use))]
    public static bool ConsoleUsePatch(Console __instance)
    {
        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && ShipStatus.Instance)
        {
            return CustomGameModeManager.ActiveMode == null ||
                   CustomGameModeManager.ActiveMode.CanUseTasks(__instance);
        }

        return true;
    }
}

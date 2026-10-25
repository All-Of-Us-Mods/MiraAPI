using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Usables;
using UnityEngine;

namespace MiraAPI.Patches.Events;

[HarmonyPatch]
public static class UsePatches
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Console), nameof(Console.Use));
        yield return AccessTools.Method(typeof(MapConsole), nameof(MapConsole.Use));
        yield return AccessTools.Method(typeof(SystemConsole), nameof(SystemConsole.Use));
        yield return AccessTools.Method(typeof(Vent), nameof(Vent.Use));
        yield return AccessTools.Method(typeof(Ladder), nameof(Ladder.Use));
        yield return AccessTools.Method(typeof(PlatformConsole), nameof(PlatformConsole.Use));
        yield return AccessTools.Method(typeof(OpenDoorConsole), nameof(OpenDoorConsole.Use));
        yield return AccessTools.Method(typeof(DoorConsole), nameof(DoorConsole.Use));
        yield return AccessTools.Method(typeof(OptionsConsole), nameof(OptionsConsole.Use));
        yield return AccessTools.Method(typeof(ZiplineConsole), nameof(ZiplineConsole.Use));
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool UsePatch(MonoBehaviour __instance)
    {
        var @event = new PlayerUseEvent(__instance.Cast<IUsable>());
        MiraEventManager.InvokeEvent(@event);

        return !@event.IsCancelled;
    }
}

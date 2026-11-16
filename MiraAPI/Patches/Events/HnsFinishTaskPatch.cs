using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(HideAndSeekManager))]
public static class HnsFinishTaskPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HideAndSeekManager.FinishTask))]
    public static void FinishTaskPostfix(HideAndSeekManager __instance, PlayerTask task)
    {
        var completeTaskEvent = new CompleteHnsTaskEvent(__instance, task.Owner, task);
        completeTaskEvent.InvokeEvent();
    }
}

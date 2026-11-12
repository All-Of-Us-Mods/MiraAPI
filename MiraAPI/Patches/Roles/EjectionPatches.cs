using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

/// <summary>
/// Patches for custom ejection messages.
/// </summary>
[HarmonyPatch(typeof(ExileController))]
internal static class EjectionPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ExileController.Begin))]
    public static void BeginPostfix(ExileController __instance)
    {
        var @event = new EjectionEvent(__instance);
        @event.InvokeEvent();

        if (!__instance.initData.networkedPlayer || !__instance.initData.networkedPlayer.Role ||
            __instance.initData.networkedPlayer.Role is not ICustomRole role)
        {
            return;
        }

        if (!GameManager.Instance.LogicOptions.GetConfirmImpostor() || role.GetCustomEjectionMessage(__instance.initData.networkedPlayer) == null)
        {
            return;
        }

        __instance.completeString = role.GetCustomEjectionMessage(__instance.initData.networkedPlayer);
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    public static class ReEnableGameplayPatch
    {
        public static void Postfix()
        {
            var @event = new BeforeRoundStartEvent(false);
            @event.InvokeEvent();

            if (@event.IsCancelled) return;

            var @event2 = new RoundStartEvent(false);
            @event2.InvokeEvent();
        }
    }
}

using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Events;

[HarmonyPatch]
public static class ReportBodyEventPatch
{
    private static bool ReportBodyEventHook(PlayerControl source, NetworkedPlayerInfo? target)
    {
        var body = target != null ? Helpers.GetBodyById(target.PlayerId) : null;

        var @event = new ReportBodyEvent(source, target, body);
        @event.InvokeEvent();
        return !@event.IsCancelled;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    public static bool PlayerControlCmdReportDeadBodyPrefix(PlayerControl __instance, NetworkedPlayerInfo? target)
    {
        return ReportBodyEventHook(__instance, target);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.LateBroadcastReliableMessage))]
    public static bool LateBroadcastReliableMessagePrefix(IGameDataMessage rpcMessage)
    {
        var reportBodyMessage = rpcMessage.TryCast<RpcReportDeadBodyMessage>();

        if (reportBodyMessage == null)
        {
            return true;
        }

        var source = AmongUsClient.Instance.FindObjectByNetId<PlayerControl>(reportBodyMessage.rpcObjectNetId);
        if (source == null)
        {
            Error("Failed to find source player for RpcReportDeadBodyMessage with NetId: " + reportBodyMessage.rpcObjectNetId);
            return true;
        }

        var target = GameData.Instance.GetPlayerById(reportBodyMessage.targetPlayerId);

        return ReportBodyEventHook(source, target);
    }
}

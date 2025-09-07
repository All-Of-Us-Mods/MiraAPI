using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Voting;

namespace MiraAPI.Patches.Voting;

[HarmonyPatch(typeof(MeetingHud))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony Convention")]
internal static class MeetingHudPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.VoteForMe))]
    public static bool VotePatch(PlayerVoteArea __instance)
    {
        var beforeVoteEvent = new BeforeVoteEvent(__instance, PlayerControl.LocalPlayer);
        MiraEventManager.InvokeEvent(beforeVoteEvent);

        if (beforeVoteEvent.IsCancelled)
        {
            return false;
        }

        var afterVoteEvent = new AfterVoteEvent(__instance, PlayerControl.LocalPlayer);
        MiraEventManager.InvokeEvent(afterVoteEvent);

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void MeetingHudStartPatch(MeetingHud __instance)
    {
        foreach (var plr in PlayerControl.AllPlayerControls)
        {
            var voteData = plr.GetVoteData();
            voteData.Votes.Clear();
            voteData.SetRemainingVotes(1);

            if (plr.Data.IsDead || plr.Data.Disconnected)
            {
                voteData.SetRemainingVotes(0);
            }

            foreach (var modifier in plr.GetModifierComponent().ActiveModifiers)
            {
                modifier.OnMeetingStart();
            }
        }

        var @event = new StartMeetingEvent(__instance);
        MiraEventManager.InvokeEvent(@event);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.OnDestroy))]
    public static void OnDestroyPatch(MeetingHud __instance)
    {
        MiraEventManager.InvokeEvent(new EndMeetingEvent(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.VotingComplete))]
    public static void VotingCompletePatch(MeetingHud __instance)
    {
        MiraEventManager.InvokeEvent(new VotingCompleteEvent(__instance));
    }

    // this is necessary because the actual ForceSkipAll method is inlined.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void ForceSkipPatch(MeetingHud __instance)
    {
        if (__instance.state is not (MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Voted))
        {
            return;
        }

        var logicOptionsNormal = GameManager.Instance.LogicOptions.Cast<LogicOptionsNormal>();
        var votingTime = logicOptionsNormal.GetVotingTime();
        if (votingTime <= 0)
        {
            return;
        }

        var num2 = __instance.discussionTimer - logicOptionsNormal.GetDiscussionTime();

        if (!AmongUsClient.Instance.AmHost || num2 < votingTime)
        {
            return;
        }

        foreach (var plr in PlayerControl.AllPlayerControls)
        {
            var voteData = plr.GetVoteData();
            voteData.SetRemainingVotes(0);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.Select))]
    public static bool SelectPatch(MeetingHud __instance, int suspectStateIdx)
    {
        var voteData = PlayerControl.LocalPlayer.GetVoteData();

        var hasVotes = voteData.VotesRemaining > 0;
        var hasVotedFor = voteData.VotedFor((byte)suspectStateIdx);

        var @event = new MeetingSelectEvent(voteData, suspectStateIdx, hasVotes && !hasVotedFor);
        MiraEventManager.InvokeEvent(@event);

        return @event.AllowSelect;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    public static bool HandleDisconnect(MeetingHud __instance, PlayerControl pc)
    {
        if (!AmongUsClient.Instance.AmHost || __instance.playerStates is null || !pc || !GameData.Instance)
        {
            return false;
        }

        var playerVoteArea = __instance.playerStates.First(pv => pv.TargetPlayerId == pc.PlayerId);
        playerVoteArea.AmDead = true;
        playerVoteArea.Overlay.gameObject.SetActive(true);

        foreach (var player in Helpers.GetAlivePlayers())
        {
            var pva = __instance.playerStates.First(pv => pv.TargetPlayerId == player.PlayerId);
            var voteData = player.GetVoteData();

            if (pva.AmDead || !voteData.VotedFor(pc.PlayerId))
            {
                continue;
            }

            voteData.Votes.RemoveAll(x=>x.Suspect==pc.PlayerId);
            voteData.VotesRemaining += 1;

            VotingUtils.RpcRemoveVote(PlayerControl.LocalPlayer, player.PlayerId, pc.PlayerId);
        }

        __instance.SetDirtyBit(1U);
        __instance.CheckForEndVoting();

        if (__instance.state == MeetingHud.VoteStates.Results)
        {
            __instance.SetupProceedButton();
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.CheckForEndVoting))]
    public static bool EndCheck(MeetingHud __instance)
    {
        var shouldEnd = !Helpers.GetAlivePlayers().Exists(plr => plr.GetVoteData().VotesRemaining > 0);

        var checkEndEvent = new CheckForEndVotingEvent(shouldEnd);
        MiraEventManager.InvokeEvent(checkEndEvent);
        shouldEnd = checkEndEvent.ForceEndVoting || checkEndEvent is { IsCancelled: false, IsVotingComplete: true };

        if (!shouldEnd)
        {
            return false;
        }

        var votes = VotingUtils.CalculateVotes();
        var exiled = VotingUtils.GetExiled(votes, out var isTie);

        var @event = new ProcessVotesEvent(votes, exiled);
        MiraEventManager.InvokeEvent(@event);

        if (@event.VotesModified)
        {
            votes = @event.Votes;
            exiled = VotingUtils.GetExiled(votes, out isTie);
        }

        if (@event.ExiledPlayerModified)
        {
            exiled = @event.ExiledPlayer;
        }

        var voterStates = new Il2CppStructArray<MeetingHud.VoterState>([
            .. votes.Select(
            v=> new MeetingHud.VoterState
            {
                VoterId = v.Voter,
                VotedForId = v.Suspect,
            })
        ]);

        __instance.RpcVotingComplete(voterStates, exiled, isTie);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.PopulateResults))]
    public static bool PopulateResultsPatch(MeetingHud __instance, ref Il2CppStructArray<MeetingHud.VoterState> states)
    {
        var votes = states.Select(x=> new CustomVote(x.VoterId, x.VotedForId)).ToList();
        var @event = new PopulateResultsEvent(votes);
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            return false;
        }

        VotingUtils.HandlePopulateResults(votes);
        return false;
    }

    // TODO: figure out a way to do host-authorization since right now any player can send RpcCastVote
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.CmdCastVote))]
    // Although this method is inlined in MeetingHud.Confirm, the next patch fixes that.
    public static bool CmdCastVoteOverridePatch(MeetingHud __instance, byte playerId, byte suspectIdx)
    {
        VotingUtils.RpcCastVote(PlayerControl.LocalPlayer, playerId, suspectIdx);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.Confirm))]
    public static bool ConfirmPatch(MeetingHud __instance, [HarmonyArgument(0)] byte suspect)
    {
        __instance.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, suspect);
        return false;
    }
}

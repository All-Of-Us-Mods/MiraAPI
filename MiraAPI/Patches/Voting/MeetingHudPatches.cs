using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.MeetingAbilities;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using UnityEngine;

namespace MiraAPI.Patches.Voting;

[HarmonyPatch(typeof(MeetingHud))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony Convention.")]
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
    public static bool VoteAreaSelectPatch(PlayerVoteArea __instance)
    {
        if (PlayerControl.LocalPlayer.Data.IsDead || __instance.AmDead || !__instance.Parent)
            return false;

        JudgeRole? judgeRole = PlayerControl.LocalPlayer.Data.Role.TryCast<JudgeRole>();
        bool flag = judgeRole && PlayerControl.LocalPlayer.PlayerId != __instance.PlayerId;
        __instance.JudgeOverruleButton?.gameObject.SetActive(flag);

        if (__instance.VoteComplete || !__instance.Parent.Select((byte)__instance.PlayerId))
            return false;

        __instance.Buttons.SetActive(true);

        float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;

        Il2CppSystem.Collections.Generic.List<UiElement> selectableElements = new();
        foreach (var btn in __instance.Buttons.GetComponentsInChildren<PassiveButton>())
        {
            selectableElements.Add(btn);
        }

        for (int i = 0; i < selectableElements.Count; i++)
        {
            var button = selectableElements[i];
            float endPos = 1.3f - 0.65f * i;
            float duration = 0.25f + 0.1f * i;
            __instance.StartCoroutine(Effects.All(Effects.Lerp(duration, (Action<float>)(t =>
                button.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * endPos, Effects.ExpOut(t))))));
        }

        ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.CancelButton, selectableElements[1], selectableElements);

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MeetingHud.Start))]
    public static void MeetingHudStartPatch(MeetingHud __instance)
    {
        MeetingButtonManager.OnMeetingStart(__instance);
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
        foreach (var meetingAbility in MeetingButtonManager.UntargetedButtons.Where(x => x.Enabled(PlayerControl.LocalPlayer.Data.Role) &&
                     (x.DisableUponVoting || x.HideUponWrapUp)))
        {
            meetingAbility.UpdateHandler(__instance);
            if (meetingAbility.Button)
            {
                meetingAbility.Button!.SetDisabled();
            }
        }
        MiraEventManager.InvokeEvent(new VotingCompleteEvent(__instance));
    }

    // this is necessary because the actual ForceSkipAll method is inlined.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.Update))]
    public static void ForceSkipPatch(MeetingHud __instance)
    {
        foreach (var targetedMeetingAbility in MeetingButtonManager.TargetedButtons.Where(x => x.Enabled(PlayerControl.LocalPlayer.Data.Role)))
        {
            targetedMeetingAbility.UpdateHandler();
        }

        foreach (var meetingAbility in MeetingButtonManager.UntargetedButtons.Where(x => x.Enabled(PlayerControl.LocalPlayer.Data.Role)))
        {
            meetingAbility.UpdateHandler(__instance);
        }

        if (__instance.state is not (MeetingHud.MeetingStates.NotVoted or MeetingHud.MeetingStates.Voted))
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
    public static bool MeetingHudSelectPatch(int suspectStateIdx)
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

        var playerVoteArea = __instance.playerStates.First(pv => pv.PlayerId == pc.PlayerId);
        playerVoteArea.AmDead = true;
        playerVoteArea.Overlay.gameObject.SetActive(true);

        foreach (var player in Helpers.GetAlivePlayers())
        {
            var pva = __instance.playerStates.First(pv => pv.PlayerId == player.PlayerId);
            var voteData = player.GetVoteData();

            if (pva.AmDead || !voteData.VotedFor(pc.PlayerId))
            {
                continue;
            }

            voteData.Votes.RemoveAll(x => x.Suspect == pc.PlayerId);
            voteData.VotesRemaining += 1;

            VotingUtils.RpcRemoveVote(PlayerControl.LocalPlayer, player.PlayerId, pc.PlayerId);
        }

        __instance.SetDirtyBit(1U);
        __instance.CheckForEndVoting();

        if (__instance.state == MeetingHud.MeetingStates.Results)
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

        __instance.RpcVotingComplete(voterStates, exiled, isTie, @event.OverruledVote, @event.OverruledNonce);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.PopulateResults))]
    public static bool PopulateResultsPatch(ref Il2CppStructArray<MeetingHud.VoterState> states)
    {
        var votes = states.Select(x => new CustomVote(x.VoterId, x.VotedForId)).ToList();

        VotingUtils.HandlePopulateResults(votes);
        return false;
    }

    // TODO: figure out a way to do host-authorization since right now any player can send RpcCastVote
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.CmdCastVote))]
    // Although this method is inlined in MeetingHud.Confirm, the next patch fixes that.
    public static bool CmdCastVoteOverridePatch(byte playerId, byte suspectIdx)
    {
        VotingUtils.RpcCastVote(PlayerControl.LocalPlayer, playerId, suspectIdx);
        return false;
    }

    // TODO: figure out a way to do host-authorization since right now any player can send RpcQueueOverruleVotes
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MeetingHud.CmdQueueOverruleVotes))]
    // Although this method is inlined in MeetingHud.Confirm, the next patch fixes that.
    public static bool CmdQueueOverruleVotesPatch(PlayerId judgePlayerId, PlayerId targetPlayerId, ushort overruleNonce)
    {
        VotingUtils.RpcQueueOverruleVotes(PlayerControl.LocalPlayer, judgePlayerId.Value, targetPlayerId.Value, overruleNonce);
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

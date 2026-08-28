using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;

namespace MiraAPI.Voting;

/// <summary>
/// Utilities used for the Mira voting system.
/// </summary>
public static class VotingUtils
{
    private const byte SkipVoteId = 253;

    /// <summary>
    /// Gets the exiled player from the list of <see cref="CustomVote"/>s. Returns <see langword="null"/> if no player is to be exiled.
    /// </summary>
    /// <param name="votes">The list of <see cref="CustomVote"/>s to check.</param>
    /// <param name="isTie">Whether the vote is a tie.</param>
    /// <returns>The player to be exiled. Will be <see langword="null"/> if no player is to be exiled.</returns>
    public static NetworkedPlayerInfo? GetExiled(List<CustomVote> votes, out bool isTie)
    {
        var max = CalculateNumVotes(votes).MaxPair(out var tie);
        isTie = tie;
        var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key);

        if (exiled is null || exiled.IsDead || exiled.Disconnected)
        {
            exiled = null;
        }

        return exiled;
    }

    /// <summary>
    /// Handles when a vote is added and allows for other mods to override/modify.
    /// </summary>
    /// <param name="voteData">The player's <see cref="PlayerVoteData"/>.</param>
    /// <param name="suspectIdx">Who the player voted for.</param>
    /// <param name="cancelVote">Whether you want the vote to commence or not.</param>
    private static void HandleVote(PlayerVoteData voteData, byte suspectIdx, bool byJudge, out bool cancelVote)
    {
        var @event = new HandleVoteEvent(voteData, suspectIdx, byJudge);
        MiraEventManager.InvokeEvent(@event);

        cancelVote = @event.PreventVote;

        if (@event.IsCancelled)
        {
            return;
        }

        if (voteData.VotesRemaining == 0 ||
            voteData.VotedFor(suspectIdx))
        {
            cancelVote = true;
            return;
        }

        if (byJudge)
        {
            voteData.SetRemainingVotes(0);
        }
        else
        {
            voteData.DecreaseRemainingVotes(1);
        }
        voteData.VoteForPlayer(suspectIdx);
    }

    /// <summary>
    /// Networks the removal of votes. Used to remove votes when a player disconnects.
    /// </summary>
    /// <param name="source">The <see cref="PlayerControl"/> who is sending the RPC. Should be the host.</param>
    /// <param name="voterId">The player who voted.</param>
    /// <param name="votedFor">The player who the voter voted for.</param>
    [MethodRpc((uint)MiraRpc.RemoveVote)]
    public static void RpcRemoveVote(PlayerControl source, byte voterId, byte votedFor)
    {
        if (!source.IsHost()) return;

        MeetingHud.Instance.playerStates.First(state => state.PlayerId == voterId).UnsetVote();

        if (PlayerControl.LocalPlayer.PlayerId != voterId)
        {
            return;
        }

        MeetingHud.Instance.playerStates.First(state => state.PlayerId == votedFor).ThumbsDown.enabled = false;

        if (!AmongUsClient.Instance.AmHost)
        {
            var voteData = PlayerControl.LocalPlayer.GetVoteData();
            voteData.DecreaseRemainingVotes(1);
            voteData.RemovePlayerVote(votedFor);
        }

        foreach (var t in MeetingHud.Instance.playerStates)
        {
            t.VoteComplete = false;
        }

        MeetingHud.Instance.SkipVoteButton.VoteComplete = false;
        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Networks the casting of a vote. We replace the vanilla solution with a custom version that works for Judge specifically.
    /// </summary>
    /// <param name="source">The <see cref="PlayerControl"/> who sent this RPC.</param>
    /// <param name="srcPlayerId">The id of the player who casted the vote.</param>
    /// <param name="suspectPlayerId">The voted player's id.</param>
    /// <param name="overruleNonce">Data that is checked by the Judge role to determine which Judge takes priority.</param>
    [MethodRpc((uint)MiraRpc.QueueOverruleVotes)]
    public static void RpcQueueOverruleVotes(PlayerControl source, byte srcPlayerId, byte suspectPlayerId, ushort overruleNonce)
    {
        CustomCastJudgeVote(srcPlayerId, suspectPlayerId, overruleNonce);
    }

    private static void CustomCastJudgeVote(PlayerId judgePlayerId, PlayerId targetPlayerId, ushort overruleNonce)
    {
        var plr = GameData.Instance.GetPlayerById(judgePlayerId.Value);
        if (!plr) return;

        var voteData = plr.Object.GetVoteData();
        if (!voteData) return;

        HandleVote(voteData, targetPlayerId.Value, true, out var cancelVote);

        // Handle local behaviour for the voter (for some reason checking AmOwner does not work for host)
        if (PlayerControl.LocalPlayer.PlayerId == judgePlayerId.Value)
        {
            if (!cancelVote) SoundManager.Instance.PlaySound(MeetingHud.Instance.VoteLockinSound, false);

            foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
            {
                playerVoteArea.ClearButtons();
            }

            MeetingHud.Instance.SkipVoteButton.ClearButtons();

            var localVoteData = PlayerControl.LocalPlayer.GetVoteData();
            if (!localVoteData || localVoteData.VotesRemaining != 0) return;

            MeetingHud.Instance.SkipVoteButton.VoteComplete = true;
            MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
        }

        if (cancelVote) return;

        // If player has no more votes, then make it show that the player has used all their votes.
        if (voteData.VotesRemaining == 0)
        {
            MeetingHud.Instance.AddOrUpdateJudgeOverrule(judgePlayerId, targetPlayerId, overruleNonce);
            MeetingHud.Instance.playerStates.First(x => x.PlayerId == judgePlayerId.Value).SetVote(targetPlayerId.Value);
        }

        // If host, then check end voting, and set votes/send chat if applicable
        if (!PlayerControl.LocalPlayer.IsHost()) return;

        MeetingHud.Instance.SetDirtyBit(1U);
        MeetingHud.Instance.CheckForEndVoting();

        if (voteData.VotesRemaining != 0) return;
        PlayerControl.LocalPlayer.RpcSendChatNote(judgePlayerId.Value, ChatNoteTypes.DidVote);
    }

    /// <summary>
    /// Networks the casting of a vote. We replace the vanilla solution with a custom version that works for the use case.
    /// </summary>
    /// <param name="source">The <see cref="PlayerControl"/> who sent this RPC.</param>
    /// <param name="srcPlayerId">The id of the player who casted the vote.</param>
    /// <param name="suspectPlayerId">The voted player's id.</param>
    [MethodRpc((uint)MiraRpc.CastVote)]
    [SuppressMessage(
        "Style",
        "IDE0060:Remove unused parameter",
        Justification = "Required parameter: The MethodRpc system mandates an InnerNetObject (or its derived class) as the first parameter for sender context, even if unused in the method body."
    )]
    public static void RpcCastVote(PlayerControl source, byte srcPlayerId, byte suspectPlayerId)
    {
        CustomCastVote(srcPlayerId, suspectPlayerId);
    }

    private static void CustomCastVote(byte srcPlayerId, byte suspectPlayerId)
    {
        var plr = GameData.Instance.GetPlayerById(srcPlayerId);
        if (!plr) return;

        var voteData = plr.Object.GetVoteData();
        if (!voteData) return;

        HandleVote(voteData, suspectPlayerId, false, out var cancelVote);

        // Handle local behaviour for the voter (for some reason checking AmOwner does not work for host)
        if (PlayerControl.LocalPlayer.PlayerId == srcPlayerId)
        {
            if (!cancelVote) SoundManager.Instance.PlaySound(MeetingHud.Instance.VoteLockinSound, false);

            foreach (var playerVoteArea in MeetingHud.Instance.playerStates)
            {
                playerVoteArea.ClearButtons();
            }

            MeetingHud.Instance.SkipVoteButton.ClearButtons();

            var localVoteData = PlayerControl.LocalPlayer.GetVoteData();
            if (!localVoteData || localVoteData.VotesRemaining != 0) return;

            MeetingHud.Instance.SkipVoteButton.VoteComplete = true;
            MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
        }

        if (cancelVote) return;

        // If player has no more votes, then make it show that the player has used all their votes.
        if (voteData.VotesRemaining == 0)
        {
            MeetingHud.Instance.playerStates.First(x => x.PlayerId == srcPlayerId).SetVote(suspectPlayerId);
        }

        // If host, then check end voting, and set votes/send chat if applicable
        if (!PlayerControl.LocalPlayer.IsHost()) return;

        MeetingHud.Instance.SetDirtyBit(1U);
        MeetingHud.Instance.CheckForEndVoting();

        if (voteData.VotesRemaining != 0) return;
        PlayerControl.LocalPlayer.RpcSendChatNote(srcPlayerId, ChatNoteTypes.DidVote);
    }

    /// <summary>
    /// Calculates the total number of votes.
    /// </summary>
    /// <param name="votes">A list of calculated <see cref="CustomVote"/>s.</param>
    /// <returns>The total votes.</returns>
    public static Dictionary<byte, float> CalculateNumVotes(IEnumerable<CustomVote> votes)
    {
        var dictionary = new Dictionary<byte, float>();

        foreach (var vote in votes.Select(v => v.Suspect))
        {
            if (!dictionary.TryAdd(vote, 1))
            {
                dictionary[vote] += 1;
            }
        }

        return dictionary;
    }

    /// <summary>
    /// Calculates votes to check if all players have voted.
    /// </summary>
    /// <returns>The <see cref="List{T}"/> of <see cref="CustomVote"/>s.</returns>
    public static List<CustomVote> CalculateVotes()
    {
        return
        [
            .. Helpers.GetAlivePlayers()
            .SelectMany(player => player.GetVoteData().Votes)
        ];
    }

    /// <summary>
    /// Handles the populating of results locally. Called by the PopulateResultsRpc.
    /// </summary>
    /// <param name="votes">The list of <see cref="CustomVote"/>s.</param>
    public static void HandlePopulateResults(List<CustomVote> votes)
    {
        var @event = new PopulateResultsEvent(votes);
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled)
        {
            return;
        }

        // If modified, these will visually change.
        votes = @event.Votes;

        MeetingHud.Instance.TitleText.text =
            TranslationController.Instance.GetString(
                StringNames.MeetingVotingResults,
                Il2CppSystem.Array.Empty<Il2CppSystem.Object>());

        var delays = new Dictionary<byte, int>();
        var num = 0;

        for (var i = 0; i < MeetingHud.Instance.playerStates.Length; i++)
        {
            var playerVoteArea = MeetingHud.Instance.playerStates[i];
            playerVoteArea.ClearForResults();
            foreach (var vote in votes)
            {
                var playerById = GameData.Instance.GetPlayerById(vote.Voter);
                if (playerById == null)
                {
                    Error($"Couldn't find player info for voter: {vote.Voter}");
                }
                else if (i == 0 && vote.Suspect == SkipVoteId)
                {
                    MeetingHud.Instance.BloopAVoteIcon(playerById, num, MeetingHud.Instance.SkippedVoting.transform);
                    num++;
                }
                else if (vote.Suspect == playerVoteArea.PlayerId)
                {
                    if (!delays.TryAdd(vote.Suspect, 0))
                    {
                        delays[vote.Suspect]++;
                    }

                    MeetingHud.Instance.BloopAVoteIcon(playerById, delays[vote.Suspect], playerVoteArea.transform);
                }
            }
        }
    }
}

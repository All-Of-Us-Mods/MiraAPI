using System;
using MiraAPI.Utilities;
using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Invoked before the dummy selects their vote. If canceled, the dummy won't vote.
/// </summary>
/// <param name="dummy">The voting dummy.</param>
public class DummyVoteEvent(DummyBehaviour dummy) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the dummy that needs to vote.
    /// </summary>
    public DummyBehaviour Dummy { get; } = dummy;

    /// <summary>
    /// Gets the instance of the dummy's <see cref="PlayerVoteData"/>.
    /// </summary>
    public PlayerVoteData VoteData { get; } = dummy.myPlayer.GetVoteData();

    /// <summary>
    /// Gets or sets a value indicating whether the dummy can vote to skip the meeting.
    /// </summary>
    public bool CanSkip { get; set; } = true;

    /// <summary>
    /// Gets or sets a predicate for validating whether the dummy can vote on a player.
    /// By default, excludes dead and disconnected players.
    /// </summary>
    public Predicate<PlayerControl> PlayerIsValid { get; set; } = x => !x.Data.IsDead && !x.Data.Disconnected;
}

using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Event that is invoked before the local player confirms a vote on a player or when skipping. This event is cancelable.
/// </summary>
public class BeforeVoteEvent : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of the voter's vote data.
    /// </summary>
    public PlayerVoteArea VoteArea { get; }

    /// <summary>
    /// Gets the player who voted.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeVoteEvent"/> class.
    /// </summary>
    /// <param name="playerVoteArea">The player vote area that was voted on.</param>
    /// <param name="voter">The player who voted.</param>
    public BeforeVoteEvent(PlayerVoteArea playerVoteArea, PlayerControl voter)
    {
        VoteArea = playerVoteArea;
        Player = voter;
    }
}

using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Event that is invoked after the local player successfully votes a player or skips. This event is not cancelable.
/// </summary>
public class AfterVoteEvent : MiraEvent
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
    /// Initializes a new instance of the <see cref="AfterVoteEvent"/> class.
    /// </summary>
    /// <param name="playerVoteArea">The player vote area that was voted on.</param>
    /// <param name="voter">The player who voted.</param>
    public AfterVoteEvent(PlayerVoteArea playerVoteArea, PlayerControl voter)
    {
        VoteArea = playerVoteArea;
        Player = voter;
    }
}

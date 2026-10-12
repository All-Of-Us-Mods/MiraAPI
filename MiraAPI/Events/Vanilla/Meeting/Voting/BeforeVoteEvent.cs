namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Event that is invoked before the local player confirms a vote on a player or when skipping. This event is cancelable.
/// </summary>
/// <param name="playerVoteArea">The <see cref="PlayerVoteArea"/> that was voted on.</param>
/// <param name="voter">The player who voted.</param>
public class BeforeVoteEvent(PlayerVoteArea playerVoteArea, PlayerControl voter) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of the voter's <see cref="PlayerVoteArea"/>.
    /// </summary>
    public PlayerVoteArea VoteArea { get; } = playerVoteArea;

    /// <summary>
    /// Gets the player who voted.
    /// </summary>
    public PlayerControl Player { get; } = voter;
}

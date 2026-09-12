namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Event that is invoked after the local player successfully votes a player or skips. This event is not cancelable.
/// </summary>
/// <param name="playerVoteArea">The <see cref="PlayerVoteArea"/> that was voted on.</param>
/// <param name="voter">The player who voted.</param>
public class AfterVoteEvent(PlayerVoteArea playerVoteArea, PlayerControl voter) : MiraEvent
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

using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// The event that is invoked when the player tries to select another player to vote.
/// </summary>
/// <param name="playerVoteData">The voter's <see cref="PlayerVoteData"/>.</param>
/// <param name="targetId">The target's playerId.</param>
/// <param name="allowSelect">Whether the player is allowed to select the target.</param>
public class MeetingSelectEvent(PlayerVoteData playerVoteData, int targetId, bool allowSelect) : MiraEvent
{
    /// <summary>
    /// Gets the instance of the voter's <see cref="PlayerVoteData"/>.
    /// </summary>
    public PlayerVoteData VoteData { get; } = playerVoteData;

    /// <summary>
    /// Gets the player id of the target.
    /// </summary>
    public int TargetId { get; } = targetId;

    /// <summary>
    /// Gets the <see cref="NetworkedPlayerInfo"/> of the target.
    /// </summary>
    public NetworkedPlayerInfo TargetPlayerInfo { get; } = GameData.Instance.GetPlayerById((byte)targetId);

    /// <summary>
    /// Gets or sets a value indicating whether the player is allowed to select the target.
    /// </summary>
    public bool AllowSelect { get; set; } = allowSelect;
}

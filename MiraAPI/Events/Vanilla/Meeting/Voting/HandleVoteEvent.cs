using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// The event that is invoked when Mira handles a vote. This is invoked before Mira's behaviour, so be cautious.
/// </summary>
/// <remarks>
/// If you intend on adding custom vote behaviour, cancel the event and do so.<br/>
/// If you are NOT canceling the event, keep in mind that Mira automatically removes a vote and adds the suspect to the <see cref="PlayerVoteData.Votes"/> list after this event is invoked.
/// </remarks>
/// <param name="playerVoteData">The voter's <see cref="PlayerVoteData"/>.</param>
/// <param name="targetId">The target's playerId.</param>
/// <param name="isOverruling">Whether the vote is actually for a Judge.</param>
public class HandleVoteEvent(PlayerVoteData playerVoteData, byte targetId, bool isOverruling = false) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of the voter's <see cref="PlayerVoteData"/>.
    /// </summary>
    public PlayerVoteData VoteData { get; } = playerVoteData;

    /// <summary>
    /// Gets the player who voted.
    /// </summary>
    public PlayerControl Player { get; } = playerVoteData.Owner;

    /// <summary>
    /// Gets the player id of the target.
    /// </summary>
    public byte TargetId { get; } = targetId;

    /// <summary>
    /// Gets or sets a value indicating whether to prevent the vote from commencing.
    /// </summary>
    public bool PreventVote { get; set; } = false;

    /// <summary>
    /// Gets the <see cref="NetworkedPlayerInfo"/> of the target.
    /// </summary>
    public NetworkedPlayerInfo TargetPlayerInfo { get; } = GameData.Instance.GetPlayerById(targetId);

    /// <summary>
    /// Gets or sets a value indicating whether the vote event is an Overrule caused by a Judge.
    /// </summary>
    public bool IsOverruling { get; set; } = isOverruling;
}

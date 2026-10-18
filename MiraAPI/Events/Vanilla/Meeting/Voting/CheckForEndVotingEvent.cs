namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Checks if voting is complete. If canceled, the default end voting logic will be skipped.
/// </summary>
/// <param name="isVotingComplete"><see langword="true"/> if voting is complete, <see langword="false"/> otherwise.</param>
public class CheckForEndVotingEvent(bool isVotingComplete) : MiraCancelableEvent
{
    /// <summary>
    /// Gets a value indicating whether default voting logic determines that voting is complete.
    /// </summary>
    public bool IsVotingComplete { get; } = isVotingComplete;

    /// <summary>
    /// Gets or sets a value indicating whether the voting should be forced to end, regardless of the default logic.
    /// </summary>
    public bool ForceEndVoting { get; set; }
}

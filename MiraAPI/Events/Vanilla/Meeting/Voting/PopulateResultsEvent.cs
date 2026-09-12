using System.Collections.Generic;
using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Called in <see cref="VotingUtils.HandlePopulateResults"/>.
/// </summary>
/// <remarks>
/// Cancelling is NOT advised but if you do plan on cancelling, please ensure you account for displaying ALL player's votes.
/// </remarks>
/// <param name="votes">The list of <see cref="CustomVote"/>s.</param>
public class PopulateResultsEvent(List<CustomVote> votes) : MiraCancelableEvent
{
    /// <summary>
    /// Gets a list of networked <see cref="CustomVote"/>s.
    /// </summary>
    public List<CustomVote> Votes { get; } = votes;
}

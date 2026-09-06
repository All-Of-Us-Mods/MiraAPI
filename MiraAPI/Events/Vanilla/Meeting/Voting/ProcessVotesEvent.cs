using System.Collections.Generic;
using System.Linq;
using MiraAPI.Voting;

namespace MiraAPI.Events.Vanilla.Meeting.Voting;

/// <summary>
/// Ran after calculating votes and before displaying the results. Only ran on the host.
/// </summary>
/// <param name="votes">The list of <see cref="CustomVote"/>s that are being processed.</param>
/// <param name="exiledPlayer">The player to be exiled. Will be <see langword="null"/> if no player is to be exiled.</param>
public class ProcessVotesEvent(List<CustomVote> votes, NetworkedPlayerInfo? exiledPlayer = null) : MiraEvent
{
    private readonly List<CustomVote> _originalVotes = [.. votes];

    /// <summary>
    /// Gets a value indicating whether the exiled player has been modified by the event.
    /// </summary>
    public bool ExiledPlayerModified { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the votes have been modified by the event.
    /// </summary>
    public bool VotesModified => !Votes.SequenceEqual(_originalVotes);

    /// <summary>
    /// Gets the list of <see cref="CustomVote"/>s that are being processed.
    /// </summary>
    public List<CustomVote> Votes { get; } = votes;

    /// <summary>
    /// Gets or sets a value indicating whether an overruling was done. Normally set by judge but can be overwritten.
    /// </summary>
    public bool OverruledVote { get; set; }

    /// <summary>
    /// Gets or sets a value specifically to determine which Judge (if any) caused the overruling. Normally set by judge but can be overwritten.
    /// </summary>
    public ushort OverruledNonce { get; set; }

    /// <summary>
    /// Gets or sets the player to be exiled. Will be <see langword="null"/> if no player is to be exiled.
    /// </summary>
    public NetworkedPlayerInfo? ExiledPlayer
    {
        get;
        set
        {
            field = value;
            ExiledPlayerModified = true;
        }
    }
    = exiledPlayer;
}

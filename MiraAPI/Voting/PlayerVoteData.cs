using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.Voting;

/// <summary>
/// Handles player votes, and removing/adding additional votes.
/// </summary>
[RegisterInIl2Cpp]
public class PlayerVoteData(nint cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the owner of this component.
    /// </summary>
    public PlayerControl Owner { get; private set; } = null!;

    /// <summary>
    /// Gets the list of <see cref="CustomVote"/>s the owner has.
    /// </summary>
    [HideFromIl2Cpp]
    public List<CustomVote> Votes { get; private set; } = [];

    /// <summary>
    /// Gets or sets the amount of votes the owner has left.
    /// </summary>
    public int VotesRemaining
    {
        get => _votesRemaining;
        set => _votesRemaining = Math.Max(0, value);
    }

    private int _votesRemaining = 1;

    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity convention.")]
    private void Awake()
    {
        Owner = GetComponent<PlayerControl>();

        if (Owner == null)
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Returns whether the owner has voted for the specified player.
    /// </summary>
    /// <param name="playerId">The target's playerId.</param>
    /// <returns><see langword="true"/> if the owner voted for the specified player, <see langword="false"/> otherwise.</returns>
    public bool VotedFor(byte playerId)
    {
        return Votes.Exists(x => x.Suspect == playerId);
    }

    /// <summary>
    /// Adds the voted player to the list.
    /// </summary>
    /// <param name="playerId">The target's playerId.</param>
    public void VoteForPlayer(byte playerId)
    {
        Votes.Add(new CustomVote(Owner.PlayerId, playerId));
    }

    /// <summary>
    /// Removes the specified <see cref="CustomVote"/>.
    /// </summary>
    /// <param name="vote">The <see cref="CustomVote"/> you would like to remove.</param>
    public void RemovePlayerVote(CustomVote vote)
    {
        Votes.Remove(vote);
    }

    /// <summary>
    /// Removes a single vote from the owner.
    /// </summary>
    /// <param name="playerId">The target's playerId.</param>
    public void RemovePlayerVote(byte playerId)
    {
        Votes.Remove(Votes.Find(x => x.Suspect == playerId));
    }

    /// <summary>
    /// Sets the player's votes remaining.
    /// </summary>
    /// <param name="votesRemaining">The amount of votes you would like to set it to.</param>
    public void SetRemainingVotes(int votesRemaining)
    {
        VotesRemaining = votesRemaining;
    }

    /// <summary>
    /// Adds votes to the owner.
    /// </summary>
    /// <param name="amount">The amount of votes you would like to add.</param>
    public void IncreaseRemainingVotes(int amount)
    {
        SetRemainingVotes(VotesRemaining + amount);
    }

    /// <summary>
    /// Removes votes from the owner.
    /// </summary>
    /// <param name="amount">The amount of votes you would like to remove.</param>
    public void DecreaseRemainingVotes(int amount)
    {
        SetRemainingVotes(VotesRemaining - amount);
    }
}

namespace MiraAPI.Networking;

/// <summary>
/// Mira RPCs.
/// </summary>
public enum MiraRpc : uint
{
    /// <summary>
    /// Syncs the game options.
    /// </summary>
    SyncGameOptions,

    /// <summary>
    /// Syncs the role options.
    /// </summary>
    SyncRoleOptions,

    /// <summary>
    /// Adds a modifier to a player.
    /// </summary>
    AddModifier,

    /// <summary>
    /// Removes a modifier from a player.
    /// </summary>
    RemoveModifier,

    /// <summary>
    /// Syncs all modifiers at once.
    /// </summary>
    SyncModifiers,

    /// <summary>
    /// Custom Murder RPC.
    /// </summary>
    CustomMurder,

    /// <summary>
    /// Custom Murder RPC Confirmation.
    /// </summary>
    ConfirmCustomMurder,

    /// <summary>
    /// Advanced Custom Murder RPC.
    /// </summary>
    AdvancedCustomMurder,

    /// <summary>
    /// Advanced Custom Murder RPC Confirmation.
    /// </summary>
    ConfirmAdvancedCustomMurder,

    /// <summary>
    /// Framed Custom Murder RPC.
    /// </summary>
    FramedCustomMurder,

    /// <summary>
    /// Framed Custom Murder RPC Confirmation.
    /// </summary>
    ConfirmFramedCustomMurder,

    /// <summary>
    /// Custom Game Over RPC.
    /// </summary>
    CustomGameOver,

    /// <summary>
    /// Custom RPC to undo a player's vote.
    /// </summary>
    RemoveVote,

    /// <summary>
    /// Custom RPC to replace Among Us CastVote.
    /// </summary>
    CastVote,

    /// <summary>
    /// Custom RPC to replace Among Us QueueOverruleVotes.
    /// </summary>
    QueueOverruleVotes,

    /// <summary>
    /// Custom RPC to sync the Gamemode option.
    /// </summary>
    SyncGamemodeOption,
}

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
    /// Trigger Animation using CustomAnimator.
    /// </summary>
    TriggerAnimation,
}

using AmongUs.GameOptions;

namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event that is invoked after a player's role is set. This event is not cancelable.
/// </summary>
/// <param name="player">The player.</param>
/// <param name="role">The new role.</param>
public class SetRoleEvent(PlayerControl player, RoleTypes role) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> whose role was changed.
    /// </summary>
    public PlayerControl Player { get; } = player;

    /// <summary>
    /// Gets the role that the player was set to.
    /// </summary>
    public RoleTypes Role { get; } = role;
}

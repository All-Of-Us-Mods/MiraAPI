namespace MiraAPI.Modifiers.Types;

/// <summary>
/// The base class for a game modifier. Game modifiers are applied at the start of the game on top of the player's role.
/// </summary>
public abstract class GameModifier : BaseModifier
{
    /// <summary>
    /// Gets the chance of the modifier being assigned to a player.
    /// </summary>
    /// <returns>An int value between 0 and 100 representing percent.</returns>
    public abstract int GetAssignmentChance();

    /// <summary>
    /// Gets the amount of players that can have this modifier in a game.
    /// </summary>
    /// <returns>An int value greater than or equal to zero.</returns>
    public abstract int GetAmountPerGame();

    /// <summary>
    /// Determines whether the modifier is valid on a role.
    /// </summary>
    /// <param name="role">The role to be checked.</param>
    /// <returns>True if the modifier is valid on the role, otherwise false.</returns>
    public virtual bool IsModifierValidOn(RoleBehaviour role) => true;
}

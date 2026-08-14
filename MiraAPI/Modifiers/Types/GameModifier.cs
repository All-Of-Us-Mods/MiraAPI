namespace MiraAPI.Modifiers.Types;

/// <summary>
/// The base class for a game modifier. Game modifiers are applied at the start of the game on top of the player's role.
/// </summary>
public abstract class GameModifier : BaseModifier
{
    /// <inheritdoc />
    public override bool ShowInFreeplay => true;

    /// <summary>
    /// Gets the chance of the modifier being assigned to a player.
    /// </summary>
    /// <returns>An <see langword="int"/> value between 0 and 100 representing percent.</returns>
    public abstract int GetAssignmentChance();

    /// <summary>
    /// Gets the amount of players that can have this modifier in a game.
    /// </summary>
    /// <returns>An <see langword="int"/> value greater than or equal to zero.</returns>
    public abstract int GetAmountPerGame();

    /// <summary>
    /// Gets the priority at which the modifier will spawn. The higher the value, the higher up on the assignment list.
    /// </summary>
    /// <returns>An <see langword="int"/> value greater than or equal to -1.</returns>
    public virtual int Priority()
    {
        return -1;
    }

    /// <summary>
    /// Determines whether the modifier is valid on a role.
    /// </summary>
    /// <param name="role">The role to be checked.</param>
    /// <returns><see langword="true"/> if the modifier is valid on the role, otherwise <see langword="false"/>.</returns>
    public virtual bool IsModifierValidOn(RoleBehaviour role)
    {
        return true;
    }

    /// <summary>
    /// Determines whether the modifier is valid on a role, which runs after players are determined but before the modifiers are given.
    /// </summary>
    /// <param name="role">The role to be checked.</param>
    /// <returns><see langword="true"/> if the modifier is valid on the role, otherwise <see langword="false"/>.</returns>
    public virtual bool IsModifierValidOnPostCheck(RoleBehaviour role)
    {
        return IsModifierValidOn(role);
    }

    /// <summary>
    /// Determines whether the player won the game with this modifier. The modifier with the highest priority will override other modifiers.
    /// </summary>
    /// <param name="reason">The reason why the game ended.</param>
    /// <returns><see langword="true"/> if the player won, <see langword="false"/> if they lost. Return <see langword="null"/> to use the player's default win condition.</returns>
    public virtual bool? DidWin(GameOverReason reason)
    {
        return null;
    }

    /// <summary>
    /// Determines whether the modifier can spawn in general, accounting for gamemodes and everything else.
    /// </summary>
    /// <returns><see langword="true"/> if the modifier is able to spawn, otherwise <see langword="false"/>.</returns>
    public virtual bool CanSpawnOnCurrentMode()
    {
        return !GameManager.Instance.IsHideAndSeek();
    }
}

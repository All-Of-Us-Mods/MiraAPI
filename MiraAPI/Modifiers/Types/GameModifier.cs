namespace MiraAPI.Modifiers.Types;
public abstract class GameModifier : BaseModifier
{
    public abstract int GetAssignmentChance();
    public abstract int GetAmountPerGame();
}
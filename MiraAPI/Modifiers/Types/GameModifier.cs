namespace MiraAPI.Modifiers.Types;
public abstract class GameModifier : BaseModifier
{
    public abstract int GetAssignmentChance();
    public abstract int GetAmountPerGame();
    public virtual bool IsModifierValidOn(RoleBehaviour role) => true;
}
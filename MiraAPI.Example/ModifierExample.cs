using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example;

[RegisterModifier]
public class ModifierExample : GameModifier
{
    public override string ModifierName => "Example";

    public override bool CanVent()
    {
        return true;
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override int GetAssignmentChance()
    {
        return 100;
    }
}
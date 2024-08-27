using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example.Modifiers;

[RegisterModifier]
public class GameModifierExample : GameModifier
{
    public override string ModifierName => "Example Game Modifier";

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
using MiraAPI.Modifiers.Types;
using MiraAPI.Translation;

namespace MiraAPI.Example.Modifiers;

public class HighPriorityModifier : GameModifier
{
    public override string ModifierName => MiraLocaleManager.Get("modifier.highPriority");

    public override string GetDescription()
    {
        return MiraLocaleManager.Get("modifier.highPriority.TabDescription");
    }

    public override int GetAssignmentChance()
    {
        return 100;
    }

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override int Priority()
    {
        return 100;
    }
}

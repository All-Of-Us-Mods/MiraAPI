using MiraAPI.GameOptions.Attributes;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example
{
    [RegisterModifier]
    public class TorchModifier : GameModifier
    {
        public override string ModifierName => "Torch";

        [ModdedNumberOption("Torch Chance", 0, 100, 10)]
        public float chance { get; } = 50;
        [ModdedNumberOption("Torch Amount", 0, 10, 1)]
        public float amount { get; } = 3;

        public override int GetAmountPerGame()
        {
            return (int)amount;
        }

        public override int GetAssignmentChance()
        {
            return (int)chance;
        }
    }
}

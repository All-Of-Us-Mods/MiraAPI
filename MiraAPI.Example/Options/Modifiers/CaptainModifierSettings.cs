using MiraAPI.Example.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example.Options.Modifiers;

public class CaptainModifierSettings : AbstractOptionGroup<CaptainModifier>
{
    public override string GroupName => "options.captainSettings";

    [ModdedNumberOption("options.captainSettings.amount", 0, 5)]
    public float Amount { get; set; } = 1;

    [ModdedNumberOption("options.captainSettings.chance", 0, 100, 10)]
    public float Chance { get; set; } = 50;

    [ModdedNumberOption("options.captainSettings.uses", 1, 5)]
    public float NumUses { get; set; } = 3;
}

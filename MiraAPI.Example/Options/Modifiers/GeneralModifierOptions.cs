using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example.Options.Modifiers;

public class GeneralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "General";
    public override uint GroupPriority => 0;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;

    [ModdedToggleOption("Some Boolean Option")]
    public bool SomeBooleanOption { get; set; } = true;

    [ModdedNumberOption("Number Option", 0, 10, 1)]
    public float NumberOption { get; set; } = 5;
}

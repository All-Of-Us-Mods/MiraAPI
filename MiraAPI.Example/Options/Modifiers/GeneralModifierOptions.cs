using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example.Options.Modifiers;

public class GeneralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "options.generalModifier";
    public override uint GroupPriority => 0;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;

    [ModdedToggleOption("options.generalModifier.someBooleanOption")]
    public bool SomeBooleanOption { get; set; } = true;

    [ModdedNumberOption("options.generalModifier.numberOption", 0, 10)]
    public float NumberOption { get; set; } = 5;
}

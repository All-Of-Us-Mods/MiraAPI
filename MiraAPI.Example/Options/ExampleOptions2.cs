using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.Example.Options;

public class ExampleOptions2 : IModdedOptionGroup
{
    public string GroupName => "Example 2";
    
    public ModdedToggleOption UseThing { get; } = new("Use Thingy 5", false);
    
    public ModdedToggleOption UseThing2 { get; } = new("Use Thingy 6", false);
    
    public ModdedStringOption Sussy { get; } = new("Sussy", 0, ["Sussy 1", "Sussy 2", "Sussy 3"]);
    
    public ModdedEnumOption SussyEnum { get; } = new("Sussy Enum", 0, typeof(SussyEnum));
    
    public ModdedNumberOption YeezyLevel { get; } = new("Yeezy Level", 50, 0, 100, 25, NumberSuffixes.Multiplier);
}

public enum SussyEnum
{
    Sussy,
    Baka,
    Imposter,
    Kanye
}
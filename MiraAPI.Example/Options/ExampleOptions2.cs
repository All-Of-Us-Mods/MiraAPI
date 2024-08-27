using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.Example.Options;

public class ExampleOptions2 : AbstractOptionGroup
{
    public override string GroupName => "Example Options 2";

    public ModdedToggleOption ToggleOpt1 { get; } = new("Toggle Option 1", false);

    public ModdedToggleOption ToggleOpt2 { get; } = new("Toggle Option 2", false)
    {
        Visible = () => OptionGroupSingleton<ExampleOptions2>.Instance.ToggleOpt1.Value
    };

    public ModdedStringOption StringOpt { get; } = new("String Opt", 0, ["Choice 1", "Choice 2", "Choice 3"]);

    public ModdedEnumOption EnumOpt { get; } = new("Enum Opt", 0, typeof(TestingEnum));
}

public enum TestingEnum
{
    Happy,
    Sad,
    Neutral
}
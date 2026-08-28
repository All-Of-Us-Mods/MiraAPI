using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using Reactor.Utilities;

namespace MiraAPI.Example.Options;

public class ExampleOptions2 : AbstractOptionGroup
{
    public override string GroupName => "options.exampleOptions2";

    public override uint GroupPriority => 0; // This group will be displayed first. The default value is uint.MaxValue.

    public override MenuCategory ParentMenu => MenuCategory.CustomTwo;

    public ModdedPlayerOption PlayerOption2 { get; } = new("options.exampleOptions2.youMustChoseAPlayer", false);
    public ModdedPlayerOption PlayerOption { get; } = new("options.exampleOptions2.ehWhatever");
    public ModdedToggleOption ToggleOpt1 { get; } = new("options.exampleOptions2.toggleOption1", false);

    public ModdedToggleOption ToggleOpt2 { get; } = new("options.exampleOptions2.toggleOption2", false)
    {
        Visible = () => OptionGroupSingleton<ExampleOptions2>.Instance.ToggleOpt1, // implicit cast from ModdedToggleOption to bool
    };

    public ModdedEnumOption<TestingData> EnumOpt { get; } = new("options.exampleOptions2.enumOpt", 0)
    {
        ChangedEvent = x => Logger<ExamplePlugin>.Info($"changed Enum Opt to {x}"),
    };
}

public enum TestingData : ulong
{
    Happy,
    Sad,
    Neutral,
}

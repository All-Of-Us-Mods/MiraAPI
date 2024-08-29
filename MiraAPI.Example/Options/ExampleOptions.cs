using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Example.Options;

public class ExampleOptions : AbstractOptionGroup
{
    public override string GroupName => "Example Options 1";
    public override Color GroupColor => Color.green;

    [ModdedToggleOption("Toggle Opt 1")]
    public bool ToggleOpt { get; set; } = false;

    [ModdedToggleOption("Toggle Opt 2")]
    public bool ToggleOpt2 { get; set; } = true;

    [ModdedNumberOption("Number Opt", min: 0, max: 10, suffixType: MiraNumberSuffixes.Percent)]
    public float NumberOpt { get; set; } = 4f;

    [ModdedStringOption("String Opt", ["Hello", "Hey", "Bye"])]
    public int StringOpt { get; set; } = 2;

    [ModdedEnumOption("Best API", typeof(BestApiEnum), ["Mira API", "Mitochondria", "Reactor"])]
    public BestApiEnum EnumOpt { get; set; } = BestApiEnum.MiraAPI;
}

public enum BestApiEnum
{
    MiraAPI,
    Mitochondria,
    Reactor
}
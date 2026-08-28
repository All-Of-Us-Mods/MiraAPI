using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Example.Options;

public class ExampleOptions : AbstractOptionGroup
{
    public override string GroupName => "options.exampleOptions1";
    public override Color GroupColor => Color.green;

    [ModdedToggleOption("options.exampleOptions1.toggleOpt1")]
    public bool ToggleOpt { get; set; } = false;

    [ModdedToggleOption("options.exampleOptions1.toggleOpt1")]
    public bool ToggleOpt2 { get; set; } = true;

    [ModdedNumberOption("options.exampleOptions1.numberOpt", min: 0, max: 10, increment: .25f, formatString: "0.00", suffixType: MiraNumberSuffixes.Percent)]
    public float NumberOpt { get; set; } = 4f;

    [ModdedEnumOption("options.exampleOptions1.bestApi", typeof(BestApi), ["Mira API", "Mitochondria", "Reactor"])]
    public BestApi Opt { get; set; } = BestApi.MiraAPI;
}

public enum BestApi
{
    MiraAPI,
    Mitochondria,
    Reactor,
}

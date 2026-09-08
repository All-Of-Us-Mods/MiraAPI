using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using System.Collections.Generic;
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

    [ModdedToggleOption("Toggle Opt 3")]
    [ModdedOptionVisiblity(nameof(ToggleOpt2))] // ToggleOpt3 will be visible only when ToggleOpt2 is true
    public bool ToggleOpt3 { get; set; } = true;

    [ModdedNumberOption("options.exampleOptions1.numberOpt", min: 0, max: 10, increment: .25f, formatString: "0.00", suffixType: MiraNumberSuffixes.Percent)]
    public float NumberOpt { get; set; } = 4f;

    [ModdedEnumOption("options.exampleOptions1.bestApi", typeof(BestApi), ["Mira API", "Mitochondria", "Reactor"])]
    public BestApi Opt { get; set; } = BestApi.MiraAPI;

    [ModdedEnumOptionList<ValueState>("Value State {0}", ["None", "Primary", "Secondary"])]
    public List<ValueState> ValueStates { get; } =
        [ValueState.Primary, ValueState.Secondary, ValueState.None, ValueState.None, ValueState.None];
}

public enum BestApi
{
    MiraAPI,
    Mitochondria,
    Reactor,
}

public enum ValueState
{
    None,
    Primary,
    Secondary,
}

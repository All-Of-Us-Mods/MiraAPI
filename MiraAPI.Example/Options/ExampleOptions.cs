using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Example.Options;

[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Demo.")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Read above.")]
[SuppressMessage("Minor Code Smell", "S3400:Methods should not return constants", Justification = "Read above.")]
public class ExampleOptions : AbstractOptionGroup
{
    public override string GroupName => "options.exampleOptions1";
    public override Color GroupColor => Color.green;

    [ModdedToggleOption("options.exampleOptions1.toggleOpt1")]
    [ModdedOptionVisibility] // Automatically resolves to using ToggleOpt1Visibility, can also target toggle options
    public bool ToggleOpt1 { get; set; } = false;

    [ModdedToggleOption("options.exampleOptions1.toggleOpt2")]
    [ModdedOptionVisibility(nameof(ToggleOpt1))] // ToggleOpt2 will be visible only when ToggleOpt1 is true
    public bool ToggleOpt2 { get; set; } = true;

    [ModdedNumberOption("options.exampleOptions1.numberOpt", min: 0, max: 10, increment: .25f, formatString: "0.00", suffixType: MiraNumberSuffixes.Percent)]
    [ModdedOptionVisibility(nameof(NumberOptVisibility))] // Can also target static methods
    public float NumberOpt { get; set; } = 4f;

    [ModdedEnumOption("options.exampleOptions1.bestApi", typeof(Api), ["Mira API", "Mitochondria", "Reactor"])]

    // These three can target both instance and static methods
    [ModdedOptionGetter(nameof(GetApi))] // Can also automatically resolve to {Option Name}Getter
    [ModdedOptionSetter(nameof(SetApi))] // Can also automatically resolve to {Option Name}Setter
    [ModdedOptionChanged(nameof(ApiChanged))] // Can also automatically resolve to {Option Name}Changed
    public Api BestApi { get; set; } = Api.MiraAPI;

    [ModdedEnumOption<Api>("options.exampleOptions1.worstApi", ["Mira API", "Mitochondria", "Reactor"])]
    public Api WorstApi { get; set; } = Api.MiraAPI;

    [ModdedEnumOptionList<ValueState>("Value State {0}", ["None", "Primary", "Secondary"])]
    public List<ValueState> ValueStates { get; } =
        [ValueState.Primary, ValueState.Secondary, ValueState.None, ValueState.None, ValueState.None];

    public bool ToggleOpt1Visibility()
    {
        return true;
    }

    public static bool NumberOptVisibility()
    {
        return true;
    }

    public static Api GetApi(Api currentValue)
    {
        return currentValue;
    }

    public static Api SetApi(Api newValue, Api oldValue)
    {
        return default;
    }

    public static void ApiChanged(Api currentValue)
    {
        // Something
    }
}

public enum Api
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

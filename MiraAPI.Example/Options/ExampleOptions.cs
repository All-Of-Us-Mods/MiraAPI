using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using UnityEngine;

namespace MiraAPI.Example.Options;

public class ExampleOptions : AbstractOptionGroup
{
    public override string GroupName => "General Group";
    public override Color GroupColor => Color.red;

    [ModdedToggleOption("Use Thing")]
    public bool UseThing { get; set; } = false;

    [ModdedToggleOption("Use another thing")]
    public bool UseAnotherThing { get; set; } = true;

    [ModdedNumberOption("Sussy level", min: 0, max: 10)]
    public float SussyLevel { get; set; } = 4f;

    [ModdedStringOption("Sus choices", ["hello", "hello 2", "hello 3"])]
    public int SusChoices { get; set; } = 2;

    [ModdedEnumOption("Enum test", typeof(TestEnum))]
    public TestEnum EnumTest { get; set; } = TestEnum.Bye;
}

public enum TestEnum
{
    Hello,
    Hi,
    Bye
}
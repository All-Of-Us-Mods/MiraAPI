using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using UnityEngine;

namespace MiraAPI.Example
{
    public class ExampleOptions : IModdedOptionGroup
    {
        public string GroupName => "General Group";
        public Color GroupColor => Color.red;

        [ModdedToggleOption("Use Thing")] public bool useThing { get; set; } = true;
        [ModdedToggleOption("Use another thing")] public bool useAnotherThing { get; set; } = false;
        [ModdedNumberOption("Sussy level", min: 0, max: 10)] public float sussyLevel { get; set; } = 4f;
        [ModdedStringOption("Sus choices", ["hello", "hello 2", "hello 3"])] public int susChoices { get; set; } = 0;
        [ModdedEnumOption("Enum test", typeof(TestEnum))] public TestEnum enumTest { get; set; } = TestEnum.Hello;
    }

    public enum TestEnum
    {
        Hello,
        Hi,
        Bye
    }
}

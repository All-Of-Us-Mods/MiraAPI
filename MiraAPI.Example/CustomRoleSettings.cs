using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.Example;

public class CustomRoleSettings : IModdedOptionGroup
{
    public string GroupName => "Custom Role";

    public Type AdvancedRole => typeof(CustomRole);

    [ModdedNumberOption("amogus", min: 3, max: 9)] public float Amogus { get; set; } = 1;
    
    [ModdedEnumOption("dwayne the rock johnson", typeof(TestEnum))] public TestEnum DwayneTheRockJohnson { get; set; } = TestEnum.Hello;
    
    public ModdedToggleOption TestToggle { get; } = new ("test toggle", false);
}
using System;
using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.Example.Options.Roles;

public class CustomRoleSettings : IModdedOptionGroup
{
    public string GroupName => "Custom Role";

    public Type AdvancedRole => typeof(CustomRole);
    
    [ModdedNumberOption("Testing level", min: 3, max: 9)] public float TestingLevel { get; set; } = 4;

}
using System;
using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example.Options.Roles;

public class CustomRoleSettings : AbstractOptionGroup
{
    public override string GroupName => "Custom Role";

    public override Type AdvancedRole => typeof(CustomRole);
    
    [ModdedNumberOption("Testing level", min: 3, max: 9)] public float TestingLevel { get; set; } = 4;

}
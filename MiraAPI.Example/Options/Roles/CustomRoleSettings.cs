using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using System;

namespace MiraAPI.Example.Options.Roles;

public class CustomRoleSettings : AbstractOptionGroup
{
    public override string GroupName => "Custom Role";

    public override Type AdvancedRole => typeof(CustomRole);

    [ModdedNumberOption("Number Opt", min: 3, max: 9)] public float NumberOpt { get; set; } = 4;

}
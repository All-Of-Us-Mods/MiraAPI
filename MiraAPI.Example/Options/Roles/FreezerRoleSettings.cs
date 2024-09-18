using System;
using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace MiraAPI.Example.Options.Roles;

public class FreezerRoleSettings : AbstractOptionGroup
{
    public override string GroupName => "Custom Role";

    public override Type AdvancedRole => typeof(FreezerRole);

    [ModdedNumberOption("Freeze Duration", 1, 15, 1, MiraNumberSuffixes.Seconds)]
    public float FreezeDuration { get; set; } = 5;

    [ModdedNumberOption("Freeze Uses", 1, 5)]
    public float FreezeUses { get; set; } = 1;
}

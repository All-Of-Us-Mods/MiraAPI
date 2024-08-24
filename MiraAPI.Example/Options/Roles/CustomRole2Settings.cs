using System;
using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example.Options.Roles;

public class CustomRole2Settings : AbstractOptionGroup
{
    public override string GroupName => "Custom Role";

    public override Type AdvancedRole => typeof(CustomRole2);
    
    [ModdedToggleOption("Teleportation ability")] public bool Teleport { get; set; } = true;

}
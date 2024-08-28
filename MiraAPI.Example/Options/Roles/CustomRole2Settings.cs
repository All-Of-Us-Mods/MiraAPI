using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using System;

namespace MiraAPI.Example.Options.Roles;

public class CustomRole2Settings : AbstractOptionGroup
{
    public override string GroupName => "Custom Role";

    public override Type AdvancedRole => typeof(FreezerRole);

    [ModdedToggleOption("Teleportation ability")] public bool Teleport { get; set; } = true;

}
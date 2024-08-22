using System;
using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.Example.Options.Roles;

public class CustomRole2Settings : IModdedOptionGroup
{
    public string GroupName => "Custom Role";

    public Type AdvancedRole => typeof(CustomRole2);
    
    [ModdedToggleOption("Teleportation ability")] public bool Teleport { get; set; } = true;

}
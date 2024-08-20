using MiraAPI.GameOptions.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example;

[RegisterCustomRole]
public class CustomRole : ImpostorRole, ICustomRole
{
    public string RoleName => "fortnite killer";
    public string RoleLongDescription => "ok so your objective is to eliminate everyone with your fortnite tactical shotgun, good luck";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.Orange;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public LoadableAsset<Sprite> OptionsScreenshot => MiraAssets.Banner;

    [ModdedNumberOption("Testing level", min: 3, max: 9, roleType: typeof(CustomRole))] public float TestingLevel { get; set; } = 4;

}
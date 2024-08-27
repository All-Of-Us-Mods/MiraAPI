using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class CustomRole : ImpostorRole, ICustomRole
{
    public string RoleName => "fortnite killer";
    public string RoleLongDescription => "ok so your objective is to eliminate everyone with your fortnite tactical shotgun, good luck";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.Black;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public LoadableAsset<Sprite> OptionsScreenshot => ExampleAssets.Banner;
    
}
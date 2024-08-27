using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class CustomRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Custom Role";
    public string RoleLongDescription => "Eliminate everyone with your special ability!";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.Black;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public LoadableAsset<Sprite> OptionsScreenshot => ExampleAssets.Banner;

}
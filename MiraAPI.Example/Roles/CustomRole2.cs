using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class CustomRole2 : CrewmateRole, ICustomRole
{
    public string RoleName => "Custom Role 2";
    public string RoleLongDescription => "Save everyone from dying!";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.AcceptedGreen;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public LoadableAsset<Sprite> OptionsScreenshot => ExampleAssets.Banner;

}
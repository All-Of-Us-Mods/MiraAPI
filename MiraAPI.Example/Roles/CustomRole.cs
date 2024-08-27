using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class CustomRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Teleporter";
    public string RoleLongDescription => "Zoom out and teleport across the map!";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Color.blue;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public LoadableAsset<Sprite> OptionsScreenshot => ExampleAssets.Banner;

}
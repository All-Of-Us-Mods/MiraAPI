using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class CustomRole3 : CrewmateRole, ICustomRole
{
    public string RoleName => "fortnite helper";
    public string RoleLongDescription => "ok so your objective is to help";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.Brown;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public LoadableAsset<Sprite> OptionsScreenshot => ExampleAssets.Banner;

}
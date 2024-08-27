using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example;

[RegisterCustomRole]
public class ExampleRole : ImpostorRole, ICustomRole
{
    public string RoleName => "fortnite killer";
    public string RoleLongDescription => "ok so your objective is to eliminate everyone \nwith your fortnite tactical shotgun";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.Orange;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public LoadableAsset<Sprite> OptionsScreenshot => MiraAssets.Empty;
}
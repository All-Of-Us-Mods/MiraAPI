using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class TeleporterRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Teleporter";
    public string RoleLongDescription => "Zoom out and teleport across the map!";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => new Color32(221, 176, 152, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    [HideFromIl2Cpp]
    public LoadableAsset<Sprite> OptionsScreenshot => ExampleAssets.Banner;
}

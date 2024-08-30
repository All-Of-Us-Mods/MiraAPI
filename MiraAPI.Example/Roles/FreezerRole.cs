using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class FreezerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Freezer";
    public string RoleLongDescription => "Freeze another player for a duration of time.";
    public string RoleDescription => this.RoleLongDescription;
    public Color RoleColor => Palette.Blue;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public LoadableAsset<Sprite> OptionsScreenshot => ExampleAssets.Banner;
    public int MaxPlayers => 2;
}

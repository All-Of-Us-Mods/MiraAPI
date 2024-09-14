using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class FreezerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Freezer";
    public string RoleLongDescription => "Freeze another player for a duration of time.";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.Blue;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new CustomRoleConfiguration(this)
    {
        OptionsScreenshot = ExampleAssets.Banner,
        MaxPlayers = 2,
    };
}

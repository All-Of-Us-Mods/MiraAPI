using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class FreezerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Freezer";
    public string RoleLongDescription => "Freeze another player for a duration of time.";
    public string RoleDescription => RoleLongDescription;
    public Color RoleColor => Palette.Blue;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = MiraAssets.ImpostorFile,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(MiraAssets.ImpostorFile.LoadAsset(), "ApiExample.Role.Impostor.FreezerRole"),
        OptionsScreenshot = ExampleAssets.Banner,
        MaxRoleCount = 2,
    };
}

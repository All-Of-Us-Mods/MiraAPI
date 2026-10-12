using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class FreezerRole : ImpostorRole, ICustomRole
{
    public string IdPart => "Freezer";
    public string IdPrefix => "ApiExample.Role.Impostor";
    public Color RoleColor => Palette.Blue;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = MiraAssets.ImpostorFile,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(MiraAssets.ImpostorFile.LoadAsset(), "ApiExample.Role.Impostor.Freezer"),
        OptionsScreenshot = ExampleAssets.Banner,
        MaxRoleCount = 2,
    };
}

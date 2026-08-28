using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Example.GameOver;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class NeutralKillerRole : ImpostorRole, ICustomRole
{
    public string IdPart => "OutcastKiller";
    public string IdPrefix => "ApiExample.Role.Outcast";
    public Color RoleColor => Color.magenta;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        CanGetKilled = true,
        CanUseVent = true,
    };

    public RoleOptionsGroup RoleOptionsGroup { get; } = new("outcast.group", Color.gray);

    [HideFromIl2Cpp]
    public TeamIntroConfiguration? IntroConfiguration { get; } = new(
        Color.gray,
        "outcast.roleTitle",
        "outcast.teamIntroDescription");

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // remove existing task header.
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<NeutralKillerGameOver>();
    }
}

using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example.Roles;

[RegisterCustomRole]
public class NeutralKillerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Neutral Killer";
    public string RoleDescription => "Neutral who can kill.";
    public string RoleLongDescription => RoleDescription;
    public Color RoleColor => Color.magenta;
    public ModdedRoleTeams Team => ModdedRoleTeams.Neutral;

    public CustomRoleConfiguration Configuration => new CustomRoleConfiguration(this)
    {
        UseVanillaKillButton = true,
        CanGetKilled = true,
        CanUseVent = true,
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        // remove existing task header.
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return GameManager.Instance.DidHumansWin(gameOverReason);
    }
}

using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class TeleporterRole : CrewmateRole, ICustomRole
{
    public string IdPart => "Teleporter";
    public string IdPrefix => "ApiExample.Role.Crewmate";
    public Color RoleColor => new Color32(221, 176, 152, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        OptionsScreenshot = ExampleAssets.Banner,
        CanModifyChance = false,
        DefaultChance = 73,
        DefaultRoleCount = 4,
    };

    public bool CanLocalPlayerSeeRole(PlayerControl player)
    {
        return true;
    }
}

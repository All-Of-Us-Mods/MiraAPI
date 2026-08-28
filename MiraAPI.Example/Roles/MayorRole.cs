using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Example.Roles;

public class MayorRole : CrewmateRole, ICustomRole
{
    public string IdPart => "Mayor";
    public string IdPrefix => "ApiExample.Role.Crewmate";
    public Color RoleColor => new Color32(221, 176, 152, 255);
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this);
}

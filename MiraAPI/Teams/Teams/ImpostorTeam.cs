using UnityEngine;

namespace MiraAPI.Teams.Teams;
public class ImpostorTeam : BaseTeam
{
    public override string TeamName => "Impostor";

    public override string TeamDescription => "Kill and sabotage";

    public override Color TeamColor => Palette.ImpostorRed;

    public override RoleTeamTypes BaseRole => RoleTeamTypes.Impostor;
}
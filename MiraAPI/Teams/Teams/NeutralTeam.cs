using UnityEngine;

namespace MiraAPI.Teams.Teams;
public class NeutralTeam : BaseTeam
{
    public override string TeamName => "Neutral";

    public override string TeamDescription => "You're on your own";

    public override Color TeamColor => Palette.DisabledGrey;

    public override RoleTeamTypes BaseRole => RoleTeamTypes.Crewmate;
}
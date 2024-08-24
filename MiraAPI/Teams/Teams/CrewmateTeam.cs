using UnityEngine;

namespace MiraAPI.Teams.Teams;
public class CrewmateTeam : BaseTeam
{
    public override string TeamName => "Crewmate";

    public override string TeamDescription => "Do your tasks";

    public override Color TeamColor => Palette.CrewmateBlue;

    public override RoleTeamTypes BaseRole => RoleTeamTypes.Crewmate;

    public override bool DidWin(GameOverReason reason)
    {
        return GameManager.Instance.DidHumansWin(reason);
    }
}
using UnityEngine;

namespace MiraAPI.Teams;
public abstract class BaseTeam
{
    public abstract string TeamName { get; }
    public abstract string TeamDescription { get; }
    public abstract Color TeamColor { get; }
    public abstract RoleTeamTypes BaseRole { get; }

    public abstract bool DidWin(GameOverReason reason);
}
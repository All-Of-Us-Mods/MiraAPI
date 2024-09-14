using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Replace the role's team intro with a custom one.
/// </summary>
public struct CustomTeamIntroScene
{
    public string TeamName;
    public string TeamDescription;
    public Color TeamColor;
    public ModdedRoleTeams BaseTeam;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTeamIntroScene"/> struct.
    /// </summary>
    /// <param name="baseTeam">The intro scene's base team.</param>
    public CustomTeamIntroScene(ModdedRoleTeams baseTeam)
    {
        BaseTeam = baseTeam;

        switch (baseTeam)
        {
            case ModdedRoleTeams.Crewmate:
                TeamName = "Crewmate";
                TeamColor = Palette.CrewmateBlue;
                TeamDescription = string.Empty;
                break;

            case ModdedRoleTeams.Impostor:
                TeamName = "Impostor";
                TeamDescription = string.Empty;
                TeamColor = Palette.ImpostorRed;
                break;
            case ModdedRoleTeams.Neutral:
                TeamName = "Neutral";
                TeamDescription = "You are Neutral. You do not have a team.";
                TeamColor = Color.gray;
                break;
        }
    }
}
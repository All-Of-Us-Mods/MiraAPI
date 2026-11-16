using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Roles;

namespace MiraAPI.VanillaEvents;

[RegisterEventContainer]
internal static class JudgeEvents
{
    [RegisterEvent(-1000)]
    public static void ProcessVotesEventHandler(ProcessVotesEvent @event)
    {
        if (!MeetingHud.Instance.TryGetWinningOverrule(
                out var judgeOverrule,
                out var networkedPlayerInfo,
                out var networkedPlayerInfo2)) return;
        Error("Judge has overruled votes!");
        @event.OverruledVote = true;
        @event.OverruledNonce = judgeOverrule.OverruleNonce;

        @event.ExiledPlayer = networkedPlayerInfo2.Role is ICustomRole { Team: not ModdedRoleTeams.Crewmate } || networkedPlayerInfo2.Role.TeamType == RoleTeamTypes.Impostor
            ? GameData.Instance.GetPlayerById(judgeOverrule.OverruledPlayerId)
            : networkedPlayerInfo;
    }
}

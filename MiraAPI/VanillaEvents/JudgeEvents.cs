using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Roles;

namespace MiraAPI.VanillaEvents;

internal static class JudgeEvents
{
    public static void Initialize()
    {
        // This is required because MiraAPI isn't an IMiraPlugin (besides in the gamemodes branch)
        MiraEventManager.RegisterEventHandler<ProcessVotesEvent>(ProcessVotesEventHandler, -1000);
    }

    private static void ProcessVotesEventHandler(ProcessVotesEvent @event)
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

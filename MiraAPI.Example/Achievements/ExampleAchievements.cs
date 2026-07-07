using System.Linq;
using MiraAPI.Achievements;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;

namespace MiraAPI.Example.Achievements;

public static class ExampleAchievements
{
    public static readonly MadKillerAchievement MadKiller = new();
    public static readonly FirstVictoryAchievement FirstWin = new();

    public static void Register()
    {
        MiraAPI.Achievements.AchievementManager.RegisterAchievement(MadKiller);
        MiraAPI.Achievements.AchievementManager.RegisterAchievement(FirstWin);

        MiraEventManager.RegisterEventHandler<AfterMurderEvent>(OnMurder);
        MiraEventManager.RegisterEventHandler<GameEndEvent>(OnGameEnd);
    }

    private static void OnMurder(AfterMurderEvent @event)
    {
        if (@event.Source.AmOwner)
        {
            MiraAPI.Achievements.AchievementManager.AddProgress(@event.Source, MadKiller.Id, 1);
        }
    }

    private static void OnGameEnd(GameEndEvent @event)
    {
        foreach (var plr in PlayerControl.AllPlayerControls.ToArray().Where(p => !p.Data.IsDead && p.AmOwner))
        {
            MiraAPI.Achievements.AchievementManager.AddProgress(plr, FirstWin.Id, 1);
        }
    }
}

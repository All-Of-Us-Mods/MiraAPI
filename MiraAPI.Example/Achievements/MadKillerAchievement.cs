using MiraAPI.Achievements;
using UnityEngine;

namespace MiraAPI.Example.Achievements;

public class MadKillerAchievement : BaseAchievement
{
    public override string Id => "mad-killer";
    public override string Title => "Mad Killer";
    public override string Description => "Kill 5 players in a single game.";
    public override AchievementTier Tier => AchievementTier.Rare;
    public override int Goal => 5;
    public override Color TitleColor => Color.red;
}

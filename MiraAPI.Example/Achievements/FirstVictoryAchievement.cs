using MiraAPI.Achievements;
using UnityEngine;

namespace MiraAPI.Example.Achievements;

public class FirstVictoryAchievement : BaseAchievement
{
    public override string Id => "first-win";
    public override string Title => "First Victory";
    public override string Description => "Win your first game.";
    public override AchievementTier Tier => AchievementTier.Common;
    public override int Goal => 1;
    public override Color TitleColor => Color.green;
}

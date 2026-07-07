using MiraAPI.Events;

namespace MiraAPI.Achievements;

/// <summary>
/// Event invoked when a player unlocks an achievement for the first time.
/// </summary>
public class AchievementUnlockedEvent : MiraEvent
{
    /// <summary>
    /// Gets the player who unlocked the achievement.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the achievement that was unlocked.
    /// </summary>
    public IAchievement Achievement { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementUnlockedEvent"/> class.
    /// </summary>
    /// <param name="player">The player who unlocked it.</param>
    /// <param name="achievement">The unlocked achievement.</param>
    public AchievementUnlockedEvent(PlayerControl player, IAchievement achievement)
    {
        Player = player;
        Achievement = achievement;
    }
}

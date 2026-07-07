using UnityEngine;

namespace MiraAPI.Achievements;

/// <summary>
/// Interface for custom achievements. Implement this to define a new achievement.
/// </summary>
public interface IAchievement
{
    /// <summary>
    /// Gets the unique identifier for this achievement.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display title shown as name prefix, e.g. "Mad Killer".
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the description of how to earn this achievement.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the tier/rarity of this achievement.
    /// </summary>
    AchievementTier Tier { get; }

    /// <summary>
    /// Gets a value indicating whether this achievement is hidden until unlocked.
    /// </summary>
    bool IsHidden => false;

    /// <summary>
    /// Gets a value indicating whether this achievement can be earned multiple times (cumulative stat).
    /// </summary>
    bool CanEarnMultiple => false;

    /// <summary>
    /// Gets the target progress value required to unlock. Default is 1 (single completion).
    /// </summary>
    int Goal => 1;

    /// <summary>
    /// Gets the color of the title when displayed on the player name.
    /// </summary>
    Color TitleColor => Color.white;

    /// <summary>
    /// Gets the current progress for a player towards this achievement.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>Current progress value.</returns>
    int GetProgress(PlayerControl player);

    /// <summary>
    /// Sets the progress for a player towards this achievement.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="value">The new progress value.</param>
    void SetProgress(PlayerControl player, int value);

    /// <summary>
    /// Checks if this achievement is unlocked for a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>True if unlocked.</returns>
    bool IsUnlocked(PlayerControl player);

    /// <summary>
    /// Evaluates whether the condition to unlock this achievement has been met.
    /// Override in subclasses for automatic condition checking.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>True if the condition is met.</returns>
    bool CheckCondition(PlayerControl player) => false;

    /// <summary>
    /// Called when this achievement is first unlocked by a player.
    /// </summary>
    /// <param name="player">The player who unlocked it.</param>
    void OnUnlocked(PlayerControl player) { }
}

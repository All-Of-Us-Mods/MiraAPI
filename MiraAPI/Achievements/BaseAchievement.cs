using MiraAPI.Achievements;
using UnityEngine;

namespace MiraAPI.Achievements;

/// <summary>
/// Abstract base class for achievements. Override members to define a custom achievement.
/// </summary>
public abstract class BaseAchievement : IAchievement
{
    /// <inheritdoc />
    public abstract string Id { get; }

    /// <inheritdoc />
    public abstract string Title { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public abstract AchievementTier Tier { get; }

    /// <inheritdoc />
    public virtual bool IsHidden => false;

    /// <inheritdoc />
    public virtual bool CanEarnMultiple => false;

    /// <inheritdoc />
    public virtual int Goal => 1;

    /// <inheritdoc />
    public virtual Color TitleColor => Color.white;

    /// <inheritdoc />
    public int GetProgress(PlayerControl player)
    {
        return AchievementManager.GetProgressData(player).GetProgress(Id);
    }

    /// <inheritdoc />
    public void SetProgress(PlayerControl player, int value)
    {
        AchievementManager.GetProgressData(player).SetProgress(Id, Mathf.Clamp(value, 0, Goal));
    }

    /// <inheritdoc />
    public bool IsUnlocked(PlayerControl player)
    {
        return GetProgress(player) >= Goal;
    }

    /// <inheritdoc />
    public virtual bool CheckCondition(PlayerControl player) => false;

    /// <inheritdoc />
    public virtual void OnUnlocked(PlayerControl player) { }
}

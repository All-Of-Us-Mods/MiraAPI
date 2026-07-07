using System.Collections.Generic;

namespace MiraAPI.Achievements;

/// <summary>
/// Stores per-achievement progress for a single player.
/// </summary>
public class AchievementProgressData
{
    private readonly Dictionary<string, int> _progress = [];

    /// <summary>
    /// Gets the progress for a specific achievement.
    /// </summary>
    /// <param name="achievementId">The achievement ID.</param>
    /// <returns>Current progress value.</returns>
    public int GetProgress(string achievementId)
    {
        return _progress.TryGetValue(achievementId, out var value) ? value : 0;
    }

    /// <summary>
    /// Sets the progress for a specific achievement.
    /// </summary>
    /// <param name="achievementId">The achievement ID.</param>
    /// <param name="value">The new progress value.</param>
    public void SetProgress(string achievementId, int value)
    {
        _progress[achievementId] = value;
    }

    internal Dictionary<string, int> ToDictionary()
    {
        return new Dictionary<string, int>(_progress);
    }

    internal void LoadFromDictionary(Dictionary<string, int> dict)
    {
        _progress.Clear();
        foreach (var kvp in dict)
        {
            _progress[kvp.Key] = kvp.Value;
        }
    }
}

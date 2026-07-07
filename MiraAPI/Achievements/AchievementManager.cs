using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using MiraAPI.Events;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Achievements;

/// <summary>
/// Manages all achievements, progress tracking, title equipping, and data persistence.
/// </summary>
public static class AchievementManager
{
    private const string AchievementDir = "mira_achievement";
    private const string AchievementFile = "Achievement.dat";

    private static readonly Dictionary<string, IAchievement> AllAchievements = [];
    private static readonly Dictionary<byte, AchievementProgressData> PlayerProgress = [];
    private static readonly Dictionary<byte, string> EquippedTitles = [];

    private static string? _currentModGuid;
    private static string? _saveFilePath;

    /// <summary>
    /// Gets a read-only view of all registered achievements.
    /// </summary>
    public static IReadOnlyDictionary<string, IAchievement> Achievements => AllAchievements;

    /// <summary>
    /// Initializes the achievement system for a specific mod. Creates/loads the encrypted data file.
    /// File is stored at: Application.persistentDataPath/mira_achievement/{ModGuid}/Achievement.dat
    /// </summary>
    /// <param name="modGuid">The mod's unique GUID, used for the subfolder name.</param>
    public static void Initialize(string modGuid)
    {
        _currentModGuid = modGuid;
        var modDir = Path.Combine(Application.persistentDataPath, AchievementDir, SanitizeFileName(modGuid));
        if (!Directory.Exists(modDir))
        {
            Directory.CreateDirectory(modDir);
        }

        _saveFilePath = Path.Combine(modDir, AchievementFile);
        LoadProgressFromFile();
    }

    /// <summary>
    /// Registers an achievement with the manager.
    /// </summary>
    /// <param name="achievement">The achievement to register.</param>
    public static void RegisterAchievement(IAchievement achievement)
    {
        if (AllAchievements.ContainsKey(achievement.Id))
        {
            Error($"Achievement with ID '{achievement.Id}' is already registered.");
            return;
        }

        AllAchievements[achievement.Id] = achievement;
        Info($"Registered achievement: {achievement.Id}");
    }

    /// <summary>
    /// Gets an achievement by its ID.
    /// </summary>
    public static bool TryGetAchievement(string id, [NotNullWhen(true)] out IAchievement? achievement)
    {
        return AllAchievements.TryGetValue(id, out achievement);
    }

    /// <summary>
    /// Gets the progress data for a player.
    /// </summary>
    public static AchievementProgressData GetProgressData(PlayerControl player)
    {
        if (!PlayerProgress.TryGetValue(player.PlayerId, out var data))
        {
            data = new AchievementProgressData();
            PlayerProgress[player.PlayerId] = data;
        }

        return data;
    }

    /// <summary>
    /// Adds progress towards an achievement for a player. Unlocks if goal is reached.
    /// </summary>
    /// <returns>True if the achievement was just unlocked.</returns>
    public static bool AddProgress(PlayerControl player, string achievementId, int amount = 1)
    {
        if (!TryGetAchievement(achievementId, out var achievement))
        {
            Error($"Achievement '{achievementId}' not found.");
            return false;
        }

        var wasUnlocked = achievement.IsUnlocked(player);
        if (wasUnlocked && !achievement.CanEarnMultiple)
        {
            return false;
        }

        var current = achievement.GetProgress(player);
        var updated = Mathf.Min(current + amount, achievement.Goal);
        achievement.SetProgress(player, updated);

        SaveProgressToFile();

        if (!wasUnlocked && updated >= achievement.Goal)
        {
            achievement.OnUnlocked(player);
            MiraEventManager.InvokeEvent(new AchievementUnlockedEvent(player, achievement));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the equipped title achievement ID for a player.
    /// </summary>
    public static string? GetEquippedTitleId(PlayerControl player)
    {
        return EquippedTitles.TryGetValue(player.PlayerId, out var titleId) ? titleId : null;
    }

    /// <summary>
    /// Gets the equipped title achievement for a player.
    /// </summary>
    public static IAchievement? GetEquippedTitle(PlayerControl player)
    {
        var titleId = GetEquippedTitleId(player);
        if (titleId != null && TryGetAchievement(titleId, out var ach) && ach.IsUnlocked(player))
        {
            return ach;
        }

        return null;
    }

    /// <summary>
    /// Sets the equipped title for a player. This only manages local state.
    /// Use an RPC to sync across the network if needed.
    /// </summary>
    public static void SetEquippedTitle(PlayerControl player, string? achievementId)
    {
        if (achievementId == null)
        {
            EquippedTitles.Remove(player.PlayerId);
            SaveProgressToFile();
            return;
        }

        if (!TryGetAchievement(achievementId, out var achievement))
        {
            Error($"Achievement '{achievementId}' not found.");
            return;
        }

        if (!achievement.IsUnlocked(player))
        {
            Error($"Achievement '{achievementId}' is not unlocked for player {player.PlayerId}.");
            return;
        }

        EquippedTitles[player.PlayerId] = achievementId;
        SaveProgressToFile();
    }

    /// <summary>
    /// Directly sets the equipped title by player ID. Does not validate or save.
    /// Useful for applying network-synced title data.
    /// </summary>
    public static void SetEquippedTitleById(byte playerId, string? achievementId)
    {
        if (achievementId == null || achievementId == "-")
        {
            EquippedTitles.Remove(playerId);
        }
        else
        {
            EquippedTitles[playerId] = achievementId;
        }
    }

    internal static void ClearAllProgress()
    {
        PlayerProgress.Clear();
        EquippedTitles.Clear();
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
        {
            if (Array.IndexOf(invalid, c) < 0)
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static void SaveProgressToFile()
    {
        if (string.IsNullOrEmpty(_saveFilePath))
        {
            return;
        }

        try
        {
            var saveData = new AchievementSaveData
            {
                Progress = PlayerProgress.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToDictionary()),
                Titles = EquippedTitles.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value),
            };

            var json = JsonSerializer.Serialize(saveData);
            var encrypted = Encrypt(json);
            File.WriteAllText(_saveFilePath, encrypted);
        }
        catch (Exception e)
        {
            Error($"Failed to save achievement data: {e.Message}");
        }
    }

    private static void LoadProgressFromFile()
    {
        if (string.IsNullOrEmpty(_saveFilePath) || !File.Exists(_saveFilePath))
        {
            return;
        }

        try
        {
            var encrypted = File.ReadAllText(_saveFilePath);
            var json = Decrypt(encrypted);
            var saveData = JsonSerializer.Deserialize<AchievementSaveData>(json);

            if (saveData == null)
            {
                return;
            }

            PlayerProgress.Clear();
            foreach (var kvp in saveData.Progress)
            {
                var progressData = new AchievementProgressData();
                progressData.LoadFromDictionary(kvp.Value);
                PlayerProgress[kvp.Key] = progressData;
            }

            EquippedTitles.Clear();
            foreach (var kvp in saveData.Titles)
            {
                EquippedTitles[kvp.Key] = kvp.Value;
            }
        }
        catch (Exception e)
        {
            Error($"Failed to load achievement data: {e.Message}");
        }
    }

    private static string Encrypt(string plainText)
    {
        var key = ComputeKey();
        var result = new StringBuilder();
        for (var i = 0; i < plainText.Length; i++)
        {
            result.Append((char)(plainText[i] ^ key[i % key.Length]));
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(result.ToString()));
    }

    private static string Decrypt(string cipherBase64)
    {
        var cipherText = Encoding.UTF8.GetString(Convert.FromBase64String(cipherBase64));
        var key = ComputeKey();
        var result = new StringBuilder();
        for (var i = 0; i < cipherText.Length; i++)
        {
            result.Append((char)(cipherText[i] ^ key[i % key.Length]));
        }

        return result.ToString();
    }

    private static string ComputeKey()
    {
        var guid = _currentModGuid ?? "MiraAPI_Achievement";
        var hash = 0;
        foreach (var c in guid)
        {
            hash = ((hash << 5) + hash) ^ c;
        }

        var key = new StringBuilder();
        for (var i = 0; i < 16; i++)
        {
            key.Append((char)('A' + (Math.Abs(hash + i * 7) % 26)));
        }

        return key.ToString();
    }

    [Serializable]
    internal class AchievementSaveData
    {
        public Dictionary<byte, Dictionary<string, int>> Progress { get; set; } = [];
        public Dictionary<byte, string> Titles { get; set; } = [];
    }
}

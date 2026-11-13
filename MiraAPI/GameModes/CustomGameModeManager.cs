using System;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.PluginLoading;

namespace MiraAPI.GameModes;

/// <summary>
/// Manages custom gamemodes.
/// </summary>
public static class CustomGameModeManager
{
    internal static readonly Dictionary<uint, AbstractGameMode> IdToModeMap = [];

    private static uint lastId;

    private static uint GetNextId()
    {
        return ++lastId;
    }

    internal static bool RegisterGameMode(Type type, in MiraPluginManager.RegistrationContext context)
    {
        if (!typeof(AbstractGameMode).IsAssignableFrom(type))
        {
            return false;
        }

        if (Activator.CreateInstance(type) is not AbstractGameMode mode)
        {
            Error($"Failed to create instance of {type.Name}");
            return false;
        }

        if (mode.HideMode)
            return true;

        var id = GetNextId();
        mode.ID = id;
        IdToModeMap.Add(id, mode);
        context.PluginInfo.GameModes.Add(id, mode);
        GameModeOption.AddOption(mode);
        return true;
    }

    /// <summary>
    /// Checks to see if the current game mode is an instance of the classic mode.
    /// </summary>
    /// <returns>True if the classic mode is the current one.</returns>
    public static bool IsClassic()
    {
        return ActiveMode != null && ActiveMode.GetType().IsAssignableTo(typeof(ClassicMode));
    }

    /// <summary>
    /// Checks to see if the current game mode is an instance of the HNS mode.
    /// </summary>
    /// <returns>True if the Hide n Seek mode is the current one.</returns>
    public static bool IsHideNSeek()
    {
        return ActiveMode != null && ActiveMode.GetType().IsAssignableTo(typeof(HideAndSeekMode));
    }

    /// <summary>
    /// Checks if a provided GameMode is the current active one.
    /// </summary>
    /// <typeparam name="T">The AbstractGameMode subclass being checked.</typeparam>
    /// <returns>Whether the provided mode is the current active one.</returns>
    public static bool IsActiveGameMode<T>()
        where T : AbstractGameMode
    {
        return ActiveMode is T;
    }

    /// <summary>
    /// Gets the current gamemode.
    /// </summary>
    public static AbstractGameMode? ActiveMode { get; private set; }

    /// <summary>
    /// Gets a gamemode from an ID.
    /// </summary>
    /// <param name="id">The ID of the gamemode to fetch.</param>
    /// <returns>The gamemode matching that ID.</returns>
    public static AbstractGameMode GetMode(uint id)
    {
        return IdToModeMap[id];
    }

    /// <summary>
    /// Finds the parent mod of a custom gamemode.
    /// </summary>
    /// <param name="gameMode">The <see cref="AbstractGameMode"/> object.</param>
    /// <returns>A <see cref="MiraPluginInfo"/> object representing the parent mod of the gamemode.</returns>
    public static MiraPluginInfo? FindParentMod(AbstractGameMode gameMode)
    {
        return MiraPluginManager.Instance.RegisteredPlugins.FirstOrDefault(plugin => plugin.GameModes.ContainsValue(gameMode));
    }

    internal static void RegisterDefaultMode()
    {
        var defaultMode = new ClassicMode { ID = 0 };
        IdToModeMap.Add(0, defaultMode);
        GameModeOption.AddOption(defaultMode);

        var hnsMode = new HideAndSeekMode() { ID = 1 };

        if (hnsMode.HideMode)
            return;

        IdToModeMap.Add(1, hnsMode);
        GameModeOption.AddOption(hnsMode);
        lastId++;
    }

    internal static void GetAndSetGameMode()
    {
        var index = GameModeOption.Value;

        if ((uint)index < (uint)GameModeOption.Values.Count)
        {
            ActiveMode = GameModeOption.Values[index];
            return;
        }

        ActiveMode = IdToModeMap[0];
        Warning($"Unable to find game mode at index {index}!");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities;

namespace MiraAPI.GameModes;

/// <summary>
/// Manages custom gamemodes.
/// </summary>
public static class CustomGameModeManager
{
    /// <summary>
    /// List of registered gamemodes.
    /// </summary>
    internal static readonly Dictionary<int, CustomGameMode> GameModes = [];

    public static bool IsDefault()
    {
        return ActiveMode?.Id == 0;
    }

    /// <summary>
    /// Gets current gamemode.
    /// </summary>
    public static CustomGameMode? ActiveMode { get; internal set; } = new DefaultMode();

    /// <summary>
    /// Set current gamemode.
    /// </summary>
    /// <param name="id">gamemode ID.</param>
    internal static void SetGameMode(int id)
    {
        if (GameModes.TryGetValue(id, out var gameMode))
        {
            ActiveMode = gameMode;
            return;
        }

        Logger<MiraApiPlugin>.Error($"No gamemode with id {id} found!");
    }

    /// <summary>
    /// Register gamemode from type.
    /// </summary>
    /// <param name="gameModeType">Type of gamemode class, should inherit from <see cref="CustomGameMode"/>.</param>
    internal static void RegisterGameMode(Type gameModeType)
    {
        if (!typeof(CustomGameMode).IsAssignableFrom(gameModeType))
        {
            Logger<MiraApiPlugin>.Warning($"{gameModeType.Name} does not inherit CustomGameMode!");
            return;
        }

        var modeObj = Activator.CreateInstance(gameModeType);

        if (modeObj is not CustomGameMode gameMode)
        {
            Logger<MiraApiPlugin>.Error($"Failed to create instance of {gameModeType.Name}");
            return;
        }

        if (GameModes.Any(x => x.Key == gameMode.Id))
        {
            Logger<MiraApiPlugin>.Error($"ID for gamemode {gameMode.Name} already exists!");
            return;
        }

        GameModes.Add(gameMode.Id, gameMode);
    }
}

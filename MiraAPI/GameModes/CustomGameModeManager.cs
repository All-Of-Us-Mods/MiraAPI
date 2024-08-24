#nullable enable
using Reactor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.GameModes;

/// <summary>
/// Manages custom gamemodes
/// </summary>
public static class CustomGameModeManager
{
    /// <summary>
    /// List of registered gamemodes
    /// </summary>
    public static readonly Dictionary<int, CustomGameMode> GameModes = [];

    private static int _nextId = 10;
    
    public static int GetNextId()
    {
        return _nextId++;
    }
    
    public static bool IsDefault()
    {
        return ActiveMode is DefaultMode;
    }

    /// <summary>
    /// Current gamemode
    /// </summary>
    public static CustomGameMode ActiveMode = new DefaultMode();

    /// <summary>
    /// Set current gamemode
    /// </summary>
    /// <param name="id">gamemode ID</param>
    public static void SetGameMode(int id)
    {
        if (GameModes.TryGetValue(id, out var gameMode))
        {
            ActiveMode = gameMode;
            return;
        }

        Logger<MiraApiPlugin>.Error($"No gamemode with id {id} found!");
    }

    /// <summary>
    /// Register gamemode from type 
    /// </summary>
    /// <param name="gameModeType">Type of gamemode class, should inherit from <see cref="CustomGameMode"/></param>
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

        var id = GetNextId();
        
        gameMode.Id = id;
        GameModes.Add(id, gameMode);
    }
}
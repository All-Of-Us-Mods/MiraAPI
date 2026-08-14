using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraAPI.GameModes;

/// <summary>
/// Manages <see cref="CustomGameMode"/>s.
/// </summary>
public static class CustomGameModeManager
{
    /// <summary>
    /// List of registered <see cref="CustomGameMode"/>.
    /// </summary>
    internal static readonly Dictionary<int, CustomGameMode> GameModes = [];

    /// <summary>
    /// Checks to see if the currently selected gamemode is vanilla.
    /// </summary>
    /// <returns>true if there is no custom gamemode selected.</returns>
    public static bool IsDefault()
    {
        return ActiveMode?.Id == 0;
    }

    /// <summary>
    /// Gets the current <see cref="CustomGameMode"/>.
    /// </summary>
    public static CustomGameMode? ActiveMode { get; internal set; } = new DefaultMode();

    /// <summary>
    /// Set current <see cref="CustomGameMode"/>.
    /// </summary>
    /// <param name="id"><see cref="CustomGameMode"/> ID.</param>
    public static void SetGameMode(int id)
    {
        if (GameModes.TryGetValue(id, out var gameMode))
        {
            ActiveMode = gameMode;
            return;
        }

        Error($"No gamemode with id {id} found!");
    }

    /// <summary>
    /// Register <see cref="CustomGameMode"/> from type.
    /// </summary>
    /// <param name="gameModeType">Type of <see cref="CustomGameMode"/>, should inherit from <see cref="CustomGameMode"/>.</param>
    internal static void RegisterGameMode(Type gameModeType)
    {
        if (!typeof(CustomGameMode).IsAssignableFrom(gameModeType))
        {
            Warning($"{gameModeType.Name} does not inherit CustomGameMode!");
            return;
        }

        var modeObj = Activator.CreateInstance(gameModeType);

        if (modeObj is not CustomGameMode gameMode)
        {
            Error($"Failed to create instance of {gameModeType.Name}");
            return;
        }

        if (GameModes.Any(x => x.Key == gameMode.Id))
        {
            Error($"ID for gamemode {gameMode.Name} already exists!");
            return;
        }

        GameModes.Add(gameMode.Id, gameMode);
    }
}

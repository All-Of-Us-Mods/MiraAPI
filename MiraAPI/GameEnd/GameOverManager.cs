using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MiraAPI.GameEnd;

/// <summary>
/// Manage <see cref="CustomGameOver"/>s.
/// </summary>
public static class GameOverManager
{
    private static readonly Dictionary<Type, int> GameOverIds = [];
    private static readonly Dictionary<int, Type> GameOverTypes = [];

    private static int _nextId = Enum.GetNames<GameOverReason>().Length;

    /// <summary>
    /// Register a <see cref="CustomGameOver"/>.
    /// </summary>
    /// <param name="gameOverType">Type of the <see cref="CustomGameOver"/>.</param>
    /// <exception cref="ArgumentException">Thrown when the type is not a subclass of <see cref="CustomGameOver"/>, is abstract, or does not have a parameterless constructor.</exception>
    /// <returns><see langword="true"/> if the game over was registered successfully, <see langword="false"/> otherwise.</returns>
    public static bool RegisterGameOver(Type gameOverType)
    {
        if (!typeof(CustomGameOver).IsAssignableFrom(gameOverType))
        {
            return false;
        }

        if (gameOverType.IsAbstract)
        {
            Error("The type must not be abstract.");
            return false;
        }

        if (gameOverType.GetConstructor(Type.EmptyTypes) == null)
        {
            Error("The type must have a parameterless constructor.");
            return false;
        }

        GameOverIds.Add(gameOverType, _nextId);
        GameOverTypes.Add(_nextId, gameOverType);
        _nextId++;
        return true;
    }

    /// <summary>
    /// Create an instance of a <see cref="CustomGameOver"/>.
    /// </summary>
    /// <param name="id">ID of the <see cref="CustomGameOver"/>.</param>
    /// <param name="customGameOver">The created instance of the <see cref="CustomGameOver"/>.</param>
    /// <returns>An instance of the <see cref="CustomGameOver"/>.</returns>
    public static bool TryGetGameOver(int id, [NotNullWhen(true)] out CustomGameOver? customGameOver)
    {
        if (GameOverTypes.TryGetValue(id, out var gameOverType))
        {
            customGameOver = Activator.CreateInstance(gameOverType) as CustomGameOver;
            return customGameOver != null;
        }

        customGameOver = null;
        return false;
    }

    /// <summary>
    /// Get the ID of a <see cref="CustomGameOver"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="CustomGameOver"/>.</typeparam>
    /// <returns>The ID of the <see cref="CustomGameOver"/>.</returns>
    public static int GetGameOverId<T>() where T : CustomGameOver
    {
        return GetGameOverId(typeof(T));
    }

    /// <summary>
    /// Get the ID of a <see cref="CustomGameOver"/>.
    /// </summary>
    /// <param name="gameOverType">Type of the <see cref="CustomGameOver"/>.</param>
    /// <returns>The ID of the <see cref="CustomGameOver"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the type is not registered.</exception>
    public static int GetGameOverId(Type gameOverType)
    {
        return GameOverIds.TryGetValue(gameOverType, out var id)
            ? id
            : throw new ArgumentException($"{gameOverType.FullName} is not a registered custom game over!");
    }
}

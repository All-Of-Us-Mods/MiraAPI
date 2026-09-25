using System.Collections.Generic;
using MiraAPI.Networking;
using Reactor.Networking.Rpc;

namespace MiraAPI.GameEnd;

/// <summary>
/// Custom Game Over.
/// </summary>
public abstract class CustomGameOver
{
    /// <summary>
    /// Gets the current <see cref="CustomGameOver"/>.
    /// </summary>
    public static CustomGameOver? Instance { get; internal set; }

    /// <summary>
    /// Verifies if the condition for this <see cref="CustomGameOver"/> is met.
    /// </summary>
    /// <param name="playerControl">The <see cref="PlayerControl"/> that requested the Game Over.</param>
    /// <param name="winners">The collection of winners.</param>
    /// <returns><see langword="true"/> if the condition is met, otherwise <see langword="false"/>.</returns>
    public virtual bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return true;
    }

    /// <summary>
    /// Runs before the base game calls <see cref="EndGameManager.SetEverythingUp"/>.
    /// </summary>
    /// <param name="endGameManager">The <see cref="EndGameManager"/> instance.</param>
    /// <returns>Return <see langword="true"/> to use the run the original method, return <see langword="false"/> to skip it.</returns>
    public virtual bool BeforeEndGameSetup(EndGameManager endGameManager)
    {
        return true;
    }

    /// <summary>
    /// Runs after the base game calls <see cref="EndGameManager.SetEverythingUp"/>.
    /// </summary>
    /// <param name="endGameManager">The <see cref="EndGameManager"/> instance.</param>
    public virtual void AfterEndGameSetup(EndGameManager endGameManager)
    {
    }

    /// <summary>
    /// Get the <see cref="global::GameOverReason"/> associated with this <see cref ="CustomGameOver"/>.
    /// </summary>
    /// <typeparam name="T">Type of the custom game over.</typeparam>
    /// <returns>The <see cref="global::GameOverReason"/> associated with the custom game over.</returns>
    public static GameOverReason GameOverReason<T>()
        where T : CustomGameOver
    {
        return (GameOverReason)GameOverManager.GetGameOverId<T>();
    }

    /// <summary>
    /// Send a custom game over.
    /// </summary>
    /// <param name="winners">A collection of winners.</param>
    /// <typeparam name="T">Type of the custom game over.</typeparam>
    public static void Trigger<T>(IEnumerable<NetworkedPlayerInfo> winners)
        where T : CustomGameOver
    {
        var reason = GameOverManager.GetGameOverId<T>();
        var data = new GameOverData(reason, [.. winners]);

        Rpc<CustomGameOverRpc>.Instance.Send(PlayerControl.LocalPlayer, data, true);
    }

    /// <summary>
    /// Implicitly convert a <see cref="CustomGameOver"/> to a <see cref="global::GameOverReason"/>.
    /// </summary>
    /// <param name="gameOver">The <see cref="CustomGameOver"/> instance.</param>
    /// <returns>The <see cref="global::GameOverReason"/> associated with the CustomGameOver.</returns>
    public static implicit operator GameOverReason(CustomGameOver gameOver)
    {
        return (GameOverReason)GameOverManager.GetGameOverId(gameOver.GetType());
    }
}

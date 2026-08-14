using InnerNet;

namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// Event that is invoked when a player leaves the game. Non cancelable.
/// </summary>
/// <param name="data">The data of the player who left.</param>
/// <param name="reason">The reason why the player left.</param>
public class PlayerLeaveEvent(ClientData data, DisconnectReasons reason) : MiraEvent
{
    /// <summary>
    /// Gets the player who left.
    /// </summary>
    public ClientData ClientData { get; } = data;

    /// <summary>
    /// Gets the reason why the player left.
    /// </summary>
    public DisconnectReasons Reason { get; } = reason;
}

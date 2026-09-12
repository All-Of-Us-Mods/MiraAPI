using InnerNet;

namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// Event that is invoked when a player joins the lobby. Non-cancelable.
/// </summary>
/// <param name="data">The data of the player who joined.</param>
public class PlayerJoinEvent(ClientData data) : MiraEvent
{
    /// <summary>
    /// Gets the player who joined.
    /// </summary>
    public ClientData ClientData { get; } = data;
}

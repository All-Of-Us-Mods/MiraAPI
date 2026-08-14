namespace MiraAPI.GameModes;

/// <summary>
/// The default gamemode of Among Us.
/// </summary>
public class DefaultMode : CustomGameMode
{
    /// <inheritdoc/>
    public override string Name => "Default";

    /// <inheritdoc/>
    public override string Description => "Default Among Us GameMode";

    /// <inheritdoc/>
    public override int Id => 0;
}

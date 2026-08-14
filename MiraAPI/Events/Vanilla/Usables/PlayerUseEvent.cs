namespace MiraAPI.Events.Vanilla.Usables;

/// <summary>
/// Event for <see cref="PlayerControl"/> using a <see cref="IUsable"/> from Vanilla Among Us. Will always be ran locally.
/// </summary>
/// <param name="usable">The <see cref="IUsable"/>.</param>
public class PlayerUseEvent(IUsable usable) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the instance of <see cref="IUsable"/> that was used.
    /// </summary>
    public IUsable Usable { get; } = usable;

    /// <summary>
    /// Gets a value indicating whether the <see cref="Usable"/> is a <see cref="Console"/>, <see cref="MapConsole"/>, or <see cref="SystemConsole"/>.
    /// </summary>
    public bool IsPrimaryConsole { get; } = usable.TryCast<Console>() || usable.TryCast<SystemConsole>() || usable.TryCast<MapConsole>();

    /// <summary>
    /// Gets a value indicating whether the <see cref="Usable"/> is a <see cref="Vent"/>.
    /// </summary>
    public bool IsVent { get; } = usable.TryCast<Vent>();
}

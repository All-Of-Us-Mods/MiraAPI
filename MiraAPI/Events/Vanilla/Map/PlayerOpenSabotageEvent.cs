namespace MiraAPI.Events.Vanilla.Map;

/// <summary>
/// Sabotage from Vanilla Among Us.
/// </summary>
/// <param name="mapBehaviour">The <see cref="global::MapBehaviour"/>.</param>
public class PlayerOpenSabotageEvent(MapBehaviour mapBehaviour) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the <see cref="global::MapBehaviour"/>.
    /// </summary>
    public MapBehaviour MapBehaviour { get; } = mapBehaviour;
}

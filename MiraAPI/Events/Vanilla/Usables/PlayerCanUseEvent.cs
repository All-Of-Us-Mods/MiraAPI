namespace MiraAPI.Events.Vanilla.Usables;

/// <summary>
/// Event for if a <see cref="PlayerControl"/> can use an <see cref="IUsable"/> from Vanilla Among Us. Will always be ran locally.
/// </summary>
/// <param name="usable">The <see cref="IUsable"/>.</param>
public class PlayerCanUseEvent(IUsable usable) : PlayerUseEvent(usable);

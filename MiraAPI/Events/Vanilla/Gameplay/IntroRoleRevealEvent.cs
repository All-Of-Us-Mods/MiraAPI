namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// The event that is invoked when the player's role is shown on the intro cutscene. Non-cancelable.
/// </summary>
/// <param name="introCutscene">The intro cutscene.</param>
public class IntroRoleRevealEvent(IntroCutscene introCutscene) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="global::IntroCutscene"/> instance.
    /// </summary>
    public IntroCutscene IntroCutscene { get; } = introCutscene;
}

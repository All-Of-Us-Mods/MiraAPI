namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// The event that is invoked when the intro cutscene has finished playing.
/// </summary>
/// <param name="introCutscene">The intro cutscene.</param>
public class IntroEndEvent(IntroCutscene introCutscene) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="global::IntroCutscene"/> instance.
    /// </summary>
    public IntroCutscene IntroCutscene { get; } = introCutscene;
}

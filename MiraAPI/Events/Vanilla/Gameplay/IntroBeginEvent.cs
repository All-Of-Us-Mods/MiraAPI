namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// The event that is invoked when the intro cutscene is shown. Non cancelable.
/// </summary>
/// <param name="introCutscene">The intro cutscene.</param>
public class IntroBeginEvent(IntroCutscene introCutscene) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="global::IntroCutscene"/> instance.
    /// </summary>
    public IntroCutscene IntroCutscene { get; } = introCutscene;
}

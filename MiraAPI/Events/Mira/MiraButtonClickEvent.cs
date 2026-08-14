using MiraAPI.Hud;

namespace MiraAPI.Events.Mira;

/// <summary>
/// Button click event for <see cref="CustomActionButton"/>s only.
/// </summary>
/// <remarks>
/// Do not use for vanilla <see cref="AbilityButton"/>s.
/// </remarks>
/// <typeparam name="T">The <see cref="CustomActionButton"/> type.</typeparam>
/// <param name="button">The <see cref="CustomActionButton"/> that was clicked.</param>
/// <param name="genericClickEvent">The generic <see cref="MiraButtonClickEvent"/> invoked before button-specific events.</param>
public sealed class MiraButtonClickEvent<T>(T button, MiraButtonClickEvent genericClickEvent) : MiraCancelableEvent where T : CustomActionButton
{
    /// <summary>
    /// Gets the <see cref="CustomActionButton"/> that was clicked.
    /// </summary>
    public T Button { get; } = button;

    /// <summary>
    /// Gets the generic <see cref="MiraButtonClickEvent"/> that is invoked before this button-specific event.
    /// </summary>
    public MiraButtonClickEvent GenericClickEvent { get; } = genericClickEvent;
}

/// <summary>
/// Button click event for <see cref="CustomActionButton"/>s only.
/// </summary>
/// <remarks>
/// Do not use for vanilla <see cref="AbilityButton"/>s.
/// </remarks>
/// <param name="button">The <see cref="CustomActionButton"/> that was clicked.</param>
public sealed class MiraButtonClickEvent(CustomActionButton button) : MiraCancelableEvent
{
    /// <summary>
    /// Gets the <see cref="CustomActionButton"/> that was clicked.
    /// </summary>
    public CustomActionButton Button { get; } = button;
}

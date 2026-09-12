using MiraAPI.Hud;

namespace MiraAPI.Events.Mira;

/// <summary>
/// Invoked when a <see cref="CustomActionButton"/> click is cancelled.
/// </summary>
/// <remarks>
/// Do not use for vanilla <see cref="AbilityButton"/>s.
/// </remarks>
/// <typeparam name="T">The <see cref="CustomActionButton"/> type.</typeparam>
/// <param name="button">The <see cref="CustomActionButton"/> whose click was cancelled.</param>
public sealed class MiraButtonCancelledEvent<T>(T button) : MiraEvent where T : CustomActionButton
{
    /// <summary>
    /// Gets the <see cref="CustomActionButton"/> whose click was cancelled.
    /// </summary>
    public T Button { get; } = button;
}

/// <summary>
/// Invoked when a <see cref="CustomActionButton"/> click is cancelled.
/// </summary>
/// <remarks>
/// Do not use for vanilla <see cref="AbilityButton"/>s.
/// </remarks>
/// <param name="button">The <see cref="CustomActionButton"/> whose click was cancelled.</param>
public sealed class MiraButtonCancelledEvent(CustomActionButton button) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="CustomActionButton"/> whose click was cancelled.
    /// </summary>
    public CustomActionButton Button { get; } = button;
}

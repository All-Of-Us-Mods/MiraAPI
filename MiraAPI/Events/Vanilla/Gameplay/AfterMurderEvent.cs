namespace MiraAPI.Events.Vanilla.Gameplay;

/// <summary>
/// Event that is invoked after a player is murdered. Only called after a successful murder. This event is not cancelable.
/// </summary>
/// <param name="source">The <see cref="PlayerControl"/> that killed the <paramref name="target"/>.</param>
/// <param name="target">The <see cref="PlayerControl"/> that was killed.</param>
/// <param name="framed">The <see cref="PlayerControl"/> that was framed.</param>
/// <param name="deadBody">The <paramref name="target"/>'s <see cref="global::DeadBody"/>, if it exists.</param>
/// <param name="isIndirect">Whether the murder was caused indirectly.</param>
/// <param name="ignoreDefense">Whether the murder ignored protection.</param>
public class AfterMurderEvent(PlayerControl source, PlayerControl target, PlayerControl? framed, DeadBody? deadBody, bool isIndirect = true, bool ignoreDefense = false) : MiraEvent
{
    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that killed the <see cref="Target"/>.
    /// </summary>
    public PlayerControl Source { get; } = source;

    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that was killed.
    /// </summary>
    public PlayerControl Target { get; } = target;

    /// <summary>
    /// Gets the <see cref="PlayerControl"/> that was framed, if any.
    /// </summary>
    public PlayerControl? Framed { get; } = framed;

    /// <summary>
    /// Gets a value indicating whether the murder was done indirectly.
    /// This is commonly used to prevent the <see cref="Source"/> <see cref="PlayerControl"/> from being killed.
    /// </summary>
    public bool IsIndirectAttack { get; } = isIndirect;

    /// <summary>
    /// Gets a value indicating whether the murder was able to bypass protective roles or general defense, such as <see cref="GuardianAngelRole"/>.
    /// </summary>
    public bool IgnoreDefense { get; } = ignoreDefense;

    /// <summary>
    /// Gets the <see cref="Target"/>'s <see cref="DeadBody"/>, if it exists.
    /// </summary>
    public DeadBody? DeadBody { get; } = deadBody;

    /// <summary>
    /// Initializes a new instance of the <see cref="AfterMurderEvent"/> class.
    /// </summary>
    /// <param name="source">The <see cref="PlayerControl"/> that killed the <paramref name="target"/>.</param>
    /// <param name="target">The <see cref="PlayerControl"/> that was killed.</param>
    /// <param name="deadBody">The <paramref name="target"/>'s <see cref="DeadBody"/>, if it exists.</param>
    /// <param name="isIndirect">Whether the murder was caused indirectly.</param>
    /// <param name="ignoreDefense">Whether the murder ignored protection.</param>
    public AfterMurderEvent(PlayerControl source, PlayerControl target, DeadBody? deadBody, bool isIndirect, bool ignoreDefense = false)
        : this(source, target, null, deadBody, isIndirect, ignoreDefense)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AfterMurderEvent"/> class.
    /// </summary>
    /// <param name="source">The <see cref="PlayerControl"/> that killed the <paramref name="target"/>.</param>
    /// <param name="target">The <see cref="PlayerControl"/> that was killed.</param>
    /// <param name="deadBody">The <paramref name="target"/>'s <see cref="DeadBody"/>, if it exists.</param>
    public AfterMurderEvent(PlayerControl source, PlayerControl target, DeadBody? deadBody)
        : this(source, target, null, deadBody)
    {
    }
}

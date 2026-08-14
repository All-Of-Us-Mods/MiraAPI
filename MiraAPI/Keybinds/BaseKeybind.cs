using System;
using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Base keybind class.
/// </summary>
public class BaseKeybind
{
    /// <summary>
    /// Gets the Rewired <see cref="InputAction"/> assigned for this keybind.
    /// </summary>
    public InputAction? RewiredInputAction { get; internal set; }

    /// <summary>
    /// Gets or sets the unique identifier for this keybind. Used in Rewired.
    /// </summary>
    public string Id { get; protected set; }

    /// <summary>
    /// Gets the currently assigned keycode.
    /// </summary>
    public KeyboardKeyCode CurrentKey => KeybindUtils.GetKeycodeByKeybind(this);

    /// <summary>
    /// Gets or sets the handler of the keybind. Invoked when the keybind is activated.
    /// </summary>
    protected Action Handler { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseKeybind"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this keybind.</param>
    public BaseKeybind(string id)
    {
        Id = id;
        Handler = () => { };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseKeybind"/> class.
    /// </summary>
    public BaseKeybind()
    {
        Handler = () => { };
    }

    /// <summary>
    /// Triggers the keybind, invoking its handler.
    /// </summary>
    public void Invoke()
    {
        Handler.Invoke();
    }

    /// <summary>
    /// Adds an <see cref="Action"/> that will be invoked when the keybind is used.
    /// </summary>
    /// <param name="action">The <see cref="Action"/> to add.</param>
    public void OnActivate(Action action)
    {
        Handler += action;
    }

    /// <summary>
    /// Removes an <see cref="Action"/> that would be invoked when the keybind is used.
    /// Not recommended to use - add checks in the method added in <see cref="OnActivate"/> itself rather than removing it from the keybind.
    /// </summary>
    /// <param name="action">The <see cref="Action"/> to remove.</param>
    public void RemoveOnActivate(Action action)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        Handler -= action;
#pragma warning restore CS8601 // Possible null reference assignment.
    }
}

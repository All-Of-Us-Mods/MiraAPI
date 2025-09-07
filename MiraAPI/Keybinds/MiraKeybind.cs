using System;
using System.Globalization;
using Reactor.Utilities;
using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Represents a registered keybind.
/// </summary>
public class MiraKeybind
{
    /// <summary>
    /// Gets name of the keybind
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the unique identifier for this keybind. Used in Rewired.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the default keycode of this keybind.
    /// </summary>
    public KeyboardKeyCode DefaultKey { get; }

    /// <summary>
    /// Gets a value indicating whether this keybind should be checked for conflicts with other exclusive keybinds using the same key.
    /// </summary>
    public bool Exclusive { get; }

    /// <summary>
    /// Gets the modifier keys assinged to this keybind.
    /// Due to Rewired limitations, there can only be 3 modifier keys.
    /// </summary>
    public ModifierKey[] ModifierKeys { get; }

    /// <summary>
    /// Gets the Rewired <see cref="InputAction"/> assinged for this keybind.
    /// </summary>
    public InputAction? RewiredInputAction { get; internal set; }

    /// <summary>
    /// Gets the currently assigned keycode.
    /// </summary>
    public KeyboardKeyCode CurrentKey => KeybindUtils.GetKeycodeByKeybind(this) ?? KeyboardKeyCode.None;

    /// <summary>
    /// Gets or sets the handler of the keybind. Invoked when the keybind is activated.
    /// </summary>
    private Action Handler { get; set; }

    internal string? SourcePluginName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraKeybind"/> class.
    /// </summary>
    /// <param name="name">The name of the keybind.</param>
    /// <param name="defaultKeycode">The default keycode.</param>
    /// <param name="modifierKeys">Up to 3 optional modifier keys.</param>
    /// <param name="exclusive">Is exclusive.</param>
    public MiraKeybind(
        string name,
        KeyboardKeyCode? defaultKeycode,
        ModifierKey[]? modifierKeys = null,
        bool exclusive = true)
    {
        Name = name;
        Id = name.ToLower(CultureInfo.InvariantCulture).Replace(' ', '_');
        DefaultKey = defaultKeycode ?? KeyboardKeyCode.None;
        ModifierKeys = modifierKeys ?? Array.Empty<ModifierKey>();
        Exclusive = exclusive;
        Handler = () => { };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Name} ({Id})";
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
        Handler -= action;
    }
}

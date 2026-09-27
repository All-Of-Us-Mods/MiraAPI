using System.Globalization;
using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Represents a registered keybind.
/// </summary>
/// <param name="name">The name of the keybind.</param>
/// <param name="defaultKeycode">The default keycode.</param>
/// <param name="modifierKeys">Up to 3 optional <see cref="ModifierKey"/>s.</param>
/// <param name="exclusive">Is exclusive.</param>
public class MiraKeybind(string name, KeyboardKeyCode? defaultKeycode, ModifierKey[]? modifierKeys = null, bool exclusive = true) : BaseKeybind(name.ToLower(CultureInfo.InvariantCulture).Replace(' ', '_'))
{
    /// <summary>
    /// Gets name of the keybind.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets the default keycode of this keybind.
    /// </summary>
    public KeyboardKeyCode DefaultKey { get; } = defaultKeycode ?? KeyboardKeyCode.None;

    /// <summary>
    /// Gets the default controller button. If <see langword="null"/>, the controller binding is left unassigned.
    /// </summary>
    public ControllerButton? DefaultControllerButton { get; }

    /// <summary>
    /// Gets the <see cref="ModifierKey"/>s assigned to this keybind.
    /// Due to Rewired limitations, there can only be 3 <see cref="ModifierKey"/>s.
    /// </summary>
    public ModifierKey[] ModifierKeys { get; } = modifierKeys ?? [];

    /// <summary>
    /// Gets a value indicating whether this keybind should be checked for conflicts with other exclusive keybinds using the same key.
    /// Active exclusive controller bindings take priority over vanilla actions during gameplay.
    /// </summary>
    public bool Exclusive { get; } = exclusive;

    internal string? SourcePluginName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiraKeybind"/> class with a controller binding.
    /// </summary>
    /// <param name="name">The name of the keybind.</param>
    /// <param name="defaultKeycode">The default keycode.</param>
    /// <param name="defaultControllerButton">The default controller button.</param>
    /// <param name="modifierKeys">Up to 3 optional <see cref="ModifierKey"/>s for the keyboard binding.</param>
    /// <param name="exclusive">Is exclusive.</param>
    public MiraKeybind(string name, KeyboardKeyCode? defaultKeycode, ControllerButton defaultControllerButton, ModifierKey[]? modifierKeys = null, bool exclusive = true) : this(name, defaultKeycode, modifierKeys, exclusive)
    {
        DefaultControllerButton = defaultControllerButton;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}
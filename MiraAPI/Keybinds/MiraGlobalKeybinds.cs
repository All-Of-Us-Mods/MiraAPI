using Rewired;

namespace MiraAPI.Keybinds;

/// <summary>
/// Class containing all default keybinds.
/// Recommended for cross-mod compatibility.
/// </summary>
[RegisterCustomKeybinds]
public static class MiraGlobalKeybinds
{
    /// <summary>
    /// Gets the keybind for primary abilities.
    /// </summary>
    public static MiraKeybind PrimaryAbility { get; } = new("Primary Ability", KeyboardKeyCode.T);

    /// <summary>
    /// Gets the keybind for secondary abilities.
    /// </summary>
    public static MiraKeybind SecondaryAbility { get; } = new("Secondary Ability", KeyboardKeyCode.Y);

    /// <summary>
    /// Gets the keybind for Tetiary abilities.
    /// </summary>
    public static MiraKeybind TertiaryAbility { get; } = new("Tertiary Ability", KeyboardKeyCode.U);

    /// <summary>
    /// Gets the keybind for primary modifier abilities.
    /// </summary>
    public static MiraKeybind ModifierPrimaryAbility { get; } = new("Modifier Primary Ability", KeyboardKeyCode.I);

    /// <summary>
    /// Gets the keybind for secondary modifier abilities.
    /// </summary>
    public static MiraKeybind ModifierSecondaryAbility { get; } = new("Modifier Secondary Ability", KeyboardKeyCode.O);

    /// <summary>
    /// Gets the keybind for Tetiary modifier abilities.
    /// </summary>
    public static MiraKeybind ModifierTertiaryAbility { get; } = new("Modifier Tertiary Ability", KeyboardKeyCode.P);
}

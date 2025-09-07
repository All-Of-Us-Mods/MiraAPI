using MiraAPI.Keybinds;
using Rewired;

namespace MiraAPI.Example;

[RegisterCustomKeybinds]
public static class ExampleKeybinds
{
    public static MiraKeybind NeutralWinKeybind { get; } = new("Neutral Win", KeyboardKeyCode.C, [ModifierKey.Shift]);
}

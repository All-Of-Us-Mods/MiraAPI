using System;

namespace MiraAPI.Keybinds;

/// <summary>
/// Used to mark a class for custom keybinds registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomKeybindsAttribute : Attribute;

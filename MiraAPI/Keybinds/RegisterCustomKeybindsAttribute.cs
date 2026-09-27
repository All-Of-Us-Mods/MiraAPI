using System;

namespace MiraAPI.Keybinds;

/// <summary>
/// Used to mark a class for <see cref="MiraKeybind"/>s registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomKeybindsAttribute : Attribute;
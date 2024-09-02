using System;

namespace MiraAPI.Hud;

/// <summary>
/// Attribute to register a button in the HUD. Necessary for the button to be recognized by Mira.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterButtonAttribute : Attribute;

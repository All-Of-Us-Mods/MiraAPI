using System;

namespace MiraAPI.Colors;

/// <summary>
/// Used to mark a class for custom colors registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomColorsAttribute : Attribute;

using System;

namespace MiraAPI.Modifiers;

/// <summary>
/// Marks a class as a modifier that can be registered with the <see cref="ModifierManager"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterModifierAttribute : Attribute;

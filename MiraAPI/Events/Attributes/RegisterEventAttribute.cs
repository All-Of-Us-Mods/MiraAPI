using System;

namespace MiraAPI.Events.Attributes;

/// <summary>
/// Used to register custom events.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterEventAttribute : Attribute;

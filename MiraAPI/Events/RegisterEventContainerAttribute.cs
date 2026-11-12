using System;

namespace MiraAPI.Events;

/// <summary>
/// Register a container of <see cref="MiraEvent"/>s.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterEventContainerAttribute : Attribute;

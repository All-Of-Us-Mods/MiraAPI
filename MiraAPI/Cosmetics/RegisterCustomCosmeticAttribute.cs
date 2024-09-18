using System;

namespace MiraAPI.Cosmetics;

[AttributeUsage(AttributeTargets.Property)]
public class RegisterCustomCosmeticAttribute(int priority = 0) : Attribute
{
    public int Priority { get; } = priority;
}

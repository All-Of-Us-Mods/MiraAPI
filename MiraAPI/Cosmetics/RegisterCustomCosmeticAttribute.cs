using System;

namespace MiraAPI.Cosmetics;

[AttributeUsage(AttributeTargets.Property)]
public class RegisterCustomCosmeticAttribute : Attribute
{
    public int Priority { get; } = 0;
    public RegisterCustomCosmeticAttribute(int priority=0)
    {
        Priority = priority;
    }
}
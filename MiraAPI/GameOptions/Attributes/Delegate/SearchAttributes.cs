using System;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// An attribute used to mark a method or property as one that returns an instance of the class it's declared in.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
public sealed class InstanceGetterAttribute : Attribute;

/// <summary>
/// An attribute used to mark a class as one whose reflection search should include private members.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SearchPrivateAttribute : Attribute;

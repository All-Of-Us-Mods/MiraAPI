using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ModdedOptionAttribute(string title, Type roleType = null) : Attribute
{
    public IModdedOption HolderOption { get; internal set; }
    
    public string Title { get; private set; } = title;
    
    protected Type RoleType { get; private set; } = roleType;

    public abstract void SetValue(object val);
    
    public abstract object GetValue();

    internal abstract IModdedOption CreateOption(object value, PropertyInfo property);
}
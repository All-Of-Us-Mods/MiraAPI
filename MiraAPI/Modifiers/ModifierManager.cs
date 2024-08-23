using System;
using System.Collections.Generic;

namespace MiraAPI.Modifiers;

public static class ModifierManager
{
    public static readonly Dictionary<uint, Type> IdToTypeModifiers = [];
    public static readonly Dictionary<Type, uint> TypeToIdModifiers = [];
    
    private static uint _nextId;
    
    private static uint GetNextId()
    {
        return _nextId++;
    }
    
    public static void RegisterModifier(Type modifierType)
    {
        if (!typeof(BaseModifier).IsAssignableFrom(modifierType))
        {
            return;
        }
        
        IdToTypeModifiers.Add(GetNextId(), modifierType);
        TypeToIdModifiers.Add(modifierType, _nextId);
    }
}
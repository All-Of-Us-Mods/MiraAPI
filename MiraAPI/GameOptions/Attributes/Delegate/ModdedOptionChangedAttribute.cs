using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute to set the <see cref="IModdedOption.ChangedEvent"/> delegate in a <see cref="ModdedOptionAttribute"/> or <see cref="ModdedOptionListAttribute"/>.
/// </summary>
/// <param name="methodName">The method to get the changed event from.</param>
/// <param name="holderType">The type the method is in.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ModdedOptionChangedAttribute(string? methodName = null, Type? holderType = null)
    : ModdedOptionMemberAttribute(methodName, holderType, "Changed")
{
    internal Delegate? GetChanged(AbstractOptionGroup group, PropertyInfo property, IModdedOption option)
    {
        if (ResolveMethod(group, property, out var type, out var instanceGetter) is not { } method)
            return null;

        var valueType = option.GetValueType();

        if (!ValidateSignature(method, typeof(void), valueType))
            return null;

        var delegateType = typeof(Action<>).MakeGenericType(valueType);
        return CompileDynamicDelegate(type, method, instanceGetter, typeof(void), [valueType], delegateType, "Changed");
    }

    internal Func<int, Delegate?>? GetListChanged(AbstractOptionGroup group, PropertyInfo property, Type valueType)
    {
        if (ResolveMethod(group, property, out var type, out var instanceGetter) is not { } method)
            return null;

        if (!ValidateListSignature(method, typeof(void), out var hasIndex, valueType))
            return null;

        var delegateType = typeof(Action<>).MakeGenericType(valueType);
        return CompileDynamicListDelegate(type, method, instanceGetter, typeof(void), [valueType], delegateType, "ListChanged", hasIndex);
    }
}

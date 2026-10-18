using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute to set the <see cref="IModdedOption.Getter"/> delegate in a <see cref="ModdedOptionAttribute"/> or <see cref="ModdedOptionListAttribute"/>.
/// </summary>
/// <param name="methodName">The method to get the getter from.</param>
/// <param name="holderType">The type the method is in.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ModdedOptionGetterAttribute(string? methodName = null, Type? holderType = null)
    : ModdedOptionMemberAttribute(methodName, holderType, "Getter")
{
    internal Delegate? GetGetter(AbstractOptionGroup group, PropertyInfo property, IModdedOption option)
    {
        if (ResolveMethod(group, property, out var type, out var instanceGetter) is not { } method)
            return null;

        var valueType = option.GetValueType();

        if (!ValidateSignature(method, valueType, valueType))
            return null;

        var delegateType = typeof(Func<,>).MakeGenericType(valueType, valueType);
        return CompileDynamicDelegate(type, method, instanceGetter, valueType, [valueType], delegateType, "Getter");
    }

    internal Func<int, Delegate?>? GetListGetter(AbstractOptionGroup group, PropertyInfo property, Type valueType)
    {
        if (ResolveMethod(group, property, out var type, out var instanceGetter) is not { } method)
            return null;

        if (!ValidateListSignature(method, valueType, out var hasIndex, valueType))
            return null;

        var delegateType = typeof(Func<,>).MakeGenericType(valueType, valueType);
        return CompileDynamicListDelegate(type, method, instanceGetter, valueType, [valueType], delegateType, "ListGetter", hasIndex);
    }
}

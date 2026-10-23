using System;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute to set the <see cref="IModdedOption.Setter"/> delegate in a <see cref="ModdedOptionAttribute"/> or <see cref="ModdedOptionListAttribute"/>.
/// </summary>
/// <param name="methodName">The method to get the setter from.</param>
/// <param name="holderType">The type the method is in.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ModdedOptionSetterAttribute(string? methodName = null, Type? holderType = null)
    : ModdedOptionMemberAttribute(methodName, holderType, "Setter")
{
    internal Delegate? GetSetter(AbstractOptionGroup group, PropertyInfo property, IModdedOption option)
    {
        if (ResolveMethod(group, property, out var type, out var instanceGetter, out var notFound) is not { } method)
        {
            if (notFound)
                Error($"Method {MemberName} does not exist in {type.FullName}");

            return null;
        }

        var valueType = option.GetValueType();

        if (!ValidateSignature(method, valueType, valueType, valueType))
            return null;

        var delegateType = typeof(Func<,,>).MakeGenericType(valueType, valueType, valueType);
        return CompileDynamicDelegate(type, method, instanceGetter, valueType, [valueType, valueType], delegateType, "Setter");
    }

    internal Func<int, Delegate?>? GetListSetter(AbstractOptionGroup group, PropertyInfo property, Type valueType)
    {
        if (ResolveMethod(group, property, out var type, out var instanceGetter, out var notFound) is not { } method)
        {
            if (notFound)
                Error($"Method {MemberName} does not exist in {type.FullName}");

            return null;
        }

        if (!ValidateListSignature(method, valueType, out var hasIndex, valueType, valueType))
            return null;

        var delegateType = typeof(Func<,,>).MakeGenericType(valueType, valueType, valueType);
        return CompileDynamicListDelegate(type, method, instanceGetter, valueType, [valueType, valueType], delegateType, "ListSetter", hasIndex);
    }
}

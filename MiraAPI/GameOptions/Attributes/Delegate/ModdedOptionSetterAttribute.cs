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
        if (ResolveMethod(group, property, out Type type, out var instanceGetter) is not { } method)
        {
            Error($"Method {MemberName} does not exist in {type.FullName}");
            return null;
        }

        var valueType = option.GetValueType();

        if (method.ReturnType != valueType)
        {
            Error($"{MemberName}'s return type is not {valueType.Name}.");
            return null;
        }

        var parameters = method.GetParameters();

        if (parameters.Length != 2)
        {
            Error($"{MemberName} must have exactly two parameters.");
            return null;
        }

        if (parameters[0].ParameterType != valueType || parameters[1].ParameterType != valueType)
        {
            Error($"{MemberName}'s parameters must both be of the type {valueType.Name}.");
            return null;
        }

        var delegateType = typeof(Func<,,>).MakeGenericType(valueType, valueType, valueType);
        return CompileDynamicDelegate(type, method, instanceGetter, valueType, [valueType, valueType], delegateType, "Setter");
    }

    internal Func<int, Delegate>? GetListSetter(AbstractOptionGroup group, PropertyInfo property, Type valueType)
    {
        if (ResolveMethod(group, property, out Type type, out var instanceGetter) is not { } method)
        {
            Error($"Method {MemberName} does not exist in {type.FullName}");
            return null;
        }

        if (method.ReturnType != valueType)
        {
            Error($"{MemberName}'s return type is not {valueType.Name}.");
            return null;
        }

        var parameters = method.GetParameters();

        if (parameters.Length is < 2 or > 3)
        {
            Error($"{MemberName} must have two parameters ({valueType.Name}, {valueType.Name}) or three parameters ({valueType.Name}, {valueType.Name}, int).");
            return null;
        }

        if (parameters[0].ParameterType != valueType || parameters[1].ParameterType != valueType)
        {
            Error($"{MemberName}'s first two parameters must both be of the type {valueType.Name}.");
            return null;
        }

        var hasIndex = parameters.Length == 3;

        if (hasIndex && parameters[2].ParameterType != typeof(int))
        {
            Error($"{MemberName}'s third parameter must be an int (the list index).");
            return null;
        }

        var delegateType = typeof(Func<,,>).MakeGenericType(valueType, valueType, valueType);
        return CompileDynamicListDelegate(type, method, instanceGetter, valueType, [valueType, valueType], delegateType, "ListSetter", hasIndex);
    }
}

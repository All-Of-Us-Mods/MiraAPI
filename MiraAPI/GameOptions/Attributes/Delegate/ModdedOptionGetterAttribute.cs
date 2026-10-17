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

        if (parameters.Length != 1)
        {
            Error($"{MemberName} must have exactly one parameter.");
            return null;
        }

        if (parameters[0].ParameterType != valueType)
        {
            Error($"{MemberName}'s parameter must be of the type {valueType.Name}.");
            return null;
        }

        var delegateType = typeof(Func<,>).MakeGenericType(valueType, valueType);
        return CompileDynamicDelegate(type, method, instanceGetter, valueType, [valueType], delegateType, "Getter");
    }

    internal Func<int, Delegate>? GetListGetter(AbstractOptionGroup group, PropertyInfo property, Type valueType)
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

        if (parameters.Length is < 1 or > 2)
        {
            Error($"{MemberName} must have one parameter ({valueType.Name}) or two parameters ({valueType.Name}, int).");
            return null;
        }

        if (parameters[0].ParameterType != valueType)
        {
            Error($"{MemberName}'s first parameter must be of the type {valueType.Name}.");
            return null;
        }

        var hasIndex = parameters.Length == 2;

        if (hasIndex && parameters[1].ParameterType != typeof(int))
        {
            Error($"{MemberName}'s second parameter must be an int (the list index).");
            return null;
        }

        var delegateType = typeof(Func<,>).MakeGenericType(valueType, valueType);
        return CompileDynamicListDelegate(type, method, instanceGetter, valueType, [valueType], delegateType, "ListGetter", hasIndex);
    }
}

using MiraAPI.GameOptions.OptionTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute to modify a property with a <see cref="ModdedOptionAttribute"/> or <see cref="ModdedOptionListAttribute"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ModdedOptionVisiblityAttribute"/> class.
/// </remarks>
/// <param name="holderType">The type the member is in.</param>
/// <param name="memberName">The member to get the visibility function from.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ModdedOptionVisiblityAttribute(Type? holderType = null, string? memberName = null) : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedOptionVisiblityAttribute"/> class.
    /// </summary>
    /// <param name="memberName">The member to get the visibility function from.</param>
    public ModdedOptionVisiblityAttribute(string memberName)
        : this(null, memberName)
    {
    }

    internal Func<bool>? GetVisibility(AbstractOptionGroup group, PropertyInfo property)
    {
        if (GetVisibilityMember(group, property, out Type type, out var instanceGet) is not { } member)
        {
            Error($"Member {memberName} does not exist in {type.FullName}");
            return null;
        }

        if (member is PropertyInfo vProperty)
        {
            if (!vProperty.PropertyType.IsAssignableTo(typeof(bool)) &&
                !vProperty.PropertyType.IsAssignableTo(typeof(ModdedOption<bool>)))
            {
                Error($"Property {memberName} is not a bool or a bool ModdedOption.");
                return null;
            }

            var indexParams = vProperty.GetIndexParameters();
            if (indexParams.Length > 0)
            {
                Error($"Property {memberName} cannot be an indexer.");
                return null;
            }

            return () => (bool)vProperty.GetValue(instanceGet?.Invoke())!;
        }
        if (member is MethodInfo vMethod)
        {
            if (vMethod.ReturnType != typeof(bool))
            {
                Error($"Method {memberName} does not return a bool.");
                return null;
            }

            var paramList = vMethod.GetParameters();
            if (paramList.Length > 0)
            {
                Error($"Method {memberName} has too many parameters.");
                return null;
            }

            return () => (bool)vMethod.Invoke(instanceGet?.Invoke(), null)!;
        }

        Error($"Member {memberName} of {type.FullName} is not valid for Visible function.");
        return null;
    }

    internal Func<int, bool>? GetListVisibility(AbstractOptionGroup group, PropertyInfo property)
    {
        if (GetVisibilityMember(group, property, out Type type, out var instanceGet) is not { } member)
        {
            Error($"Member {memberName} does not exist in {type.FullName}");
            return null;
        }

        if (member is PropertyInfo vProperty)
        {
            if (!vProperty.PropertyType.IsAssignableTo(typeof(bool)) &&
                !vProperty.PropertyType.IsAssignableTo(typeof(ModdedOption<bool>)) &&
                !vProperty.PropertyType.IsAssignableTo(typeof(IList<bool>)))
            {
                Error($"Property {memberName} is not a bool value or indexer, a list of bools, or a bool ModdedOption.");
                return null;
            }

            var indexParams = vProperty.GetIndexParameters();
            if (indexParams.Length > 1)
            {
                Error($"Indexer {memberName} has too many parameters.");
                return null;
            }

            if (vProperty.PropertyType.IsAssignableTo(typeof(IList<bool>)))
            {
                if (indexParams.Length != 0)
                {
                    Error($"Property {memberName} cannot be an indexer of lists of bools.");
                    return null;
                }

                return i => ((IList<bool>)vProperty.GetValue(instanceGet?.Invoke())!)[i];
            }

            if (indexParams.Length == 0)
            {
                return _ => (bool)vProperty.GetValue(instanceGet?.Invoke())!;
            }
            if (indexParams[0].ParameterType != typeof(int))
            {
                Error($"Indexer {memberName}'s parameter is not an int.");
                return null;
            }

            return i => (bool)vProperty.GetValue(instanceGet?.Invoke(), [i])!;
        }
        if (member is MethodInfo vMethod)
        {
            if (vMethod.ReturnType != typeof(bool))
            {
                Error($"Method {memberName} does not return a bool.");
                return null;
            }

            var paramList = vMethod.GetParameters();
            if (paramList.Length > 1)
            {
                Error($"Method {memberName} has too many parameters.");
                return null;
            }

            if (paramList.Length == 0)
            {
                return _ => (bool)vMethod.Invoke(instanceGet?.Invoke(), null)!;
            }
            if (paramList[0].ParameterType != typeof(int))
            {
                Error($"Method {memberName}'s parameter is not an int.");
                return null;
            }

            return i => (bool)vMethod.Invoke(instanceGet?.Invoke(), [i])!;
        }

        Error($"Member {memberName} of {type.FullName} is not valid for Visible function.");
        return null;
    }

#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
    private MemberInfo? GetVisibilityMember(AbstractOptionGroup group, PropertyInfo property, out Type type, out Func<object>? instanceGet)
    {
        Type groupType = group.GetType();
        BindingFlags flags = BindingFlags.Static | BindingFlags.Public;

        instanceGet = null;
        type = holderType ?? groupType;
        memberName ??= $"{property.Name}Visible";
        if (type.IsAssignableTo(typeof(AbstractOptionGroup)))
        {
            flags |= BindingFlags.Instance;

            if (type == groupType)
            {
                flags |= BindingFlags.NonPublic;
                instanceGet = () => group;
            }
            else
            {
                var instanceProp = typeof(OptionGroupSingleton<>).MakeGenericType(type)
                    .GetProperty(nameof(OptionGroupSingleton<>.Instance))!;
                instanceGet = () => instanceProp.GetValue(null)!;
            }
        }
        MemberInfo? member = type.GetMember(memberName, flags).FirstOrDefault();
        if (instanceGet != null)
        {
            MethodInfo? method = (member as PropertyInfo)?.GetGetMethod()
                               ?? member as MethodInfo;
            if (method != null && method.IsStatic)
            {
                instanceGet = null;
            }
        }
        return member;
    }
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
}

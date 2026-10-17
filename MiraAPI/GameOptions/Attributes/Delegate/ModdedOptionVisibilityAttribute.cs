using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute to set the <see cref="IModdedOption.Visible"/> delegate in a <see cref="ModdedOptionAttribute"/> or <see cref="ModdedOptionListAttribute"/>.
/// </summary>
/// <param name="memberName">The member to get the visibility function from.</param>
/// <param name="holderType">The type the member is in.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ModdedOptionVisibilityAttribute(string? memberName = null, Type? holderType = null)
    : ModdedOptionMemberAttribute(memberName, holderType, "Visibility")
{
    private static readonly MethodInfo ModdedOptionBoolValueGetter = typeof(ModdedOption<bool>).GetProperty(nameof(ModdedOption<>.Value))!.GetGetMethod()!;
    private static readonly MethodInfo ListBoolIndexerGetter = typeof(IList<bool>).GetProperty("Item")!.GetGetMethod()!;

    internal Func<bool>? GetVisibility(AbstractOptionGroup group, PropertyInfo property)
    {
        if (ResolveMember(group, property, out Type type, out var instanceGetter) is not { } member)
        {
            Error($"Member {MemberName} does not exist in {type.FullName}");
            return null;
        }

        MethodInfo? targetMethod;
        Action<ILGenerator>? postProcess = null;

        if (member is PropertyInfo vProperty)
        {
            if (vProperty.PropertyType.IsAssignableTo(typeof(ModdedOption<bool>)))
            {
                postProcess = il => il.Emit(OpCodes.Callvirt, ModdedOptionBoolValueGetter);
            }
            else if (!vProperty.PropertyType.IsAssignableTo(typeof(bool)))
            {
                Error($"Property {MemberName} is not a bool or a bool ModdedOption.");
                return null;
            }

            if (vProperty.GetIndexParameters().Length > 0)
            {
                Error($"Property {MemberName} cannot be an indexer.");
                return null;
            }

            targetMethod = vProperty.GetGetMethod(true);

            if (targetMethod is null)
            {
                Error($"Property {MemberName} does not have a getter.");
                return null;
            }
        }
        else if (member is MethodInfo vMethod)
        {
            if (vMethod.ReturnType != typeof(bool))
            {
                Error($"Method {MemberName} does not return a bool.");
                return null;
            }

            if (vMethod.GetParameters().Length > 0)
            {
                Error($"Method {MemberName} has too many parameters.");
                return null;
            }

            targetMethod = vMethod;
        }
        else if (member is FieldInfo field)
        {
            if (field.FieldType != typeof(bool))
            {
                Error($"Field {MemberName} is not a bool.");
                return null;
            }

            var dynamicMethod = new DynamicMethod($"GetVisibility_{type.Name}_{field.Name}", typeof(bool), Type.EmptyTypes, type, true);
            var il = dynamicMethod.GetILGenerator();

            if (instanceGetter != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, instanceGetter);
                il.Emit(OpCodes.Ldfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, field);
            }

            il.Emit(OpCodes.Ret);
            return dynamicMethod.CreateDelegate<Func<bool>>();
        }
        else
        {
            Error($"Member {MemberName} of {type.FullName} is not valid for Visible function.");
            return null;
        }

        return (Func<bool>)CompileDynamicDelegate(type, targetMethod, instanceGetter, typeof(bool), Type.EmptyTypes, typeof(Func<bool>), "Visibility", false, postProcess);
    }

    internal Func<int, Delegate>? GetListVisibility(AbstractOptionGroup group, PropertyInfo property)
    {
        if (ResolveMember(group, property, out Type type, out var instanceGetter) is not { } member)
        {
            Error($"Member {MemberName} does not exist in {type.FullName}");
            return null;
        }

        MethodInfo? targetMethod;
        Action<ILGenerator>? postProcess = null;
        Action<ILGenerator, int>? postProcessList = null;

        var hasIndexParam = false;

        if (member is PropertyInfo vProperty)
        {
            var isListBool = false;

            if (vProperty.PropertyType.IsAssignableTo(typeof(IList<bool>)))
            {
                isListBool = true;
                postProcessList = (il, index) =>
                {
                    il.Emit(OpCodes.Ldc_I4, index);
                    il.Emit(OpCodes.Callvirt, ListBoolIndexerGetter);
                };
            }
            else if (vProperty.PropertyType.IsAssignableTo(typeof(ModdedOption<bool>)))
            {
                postProcess = il => il.Emit(OpCodes.Callvirt, ModdedOptionBoolValueGetter);
            }
            else if (!vProperty.PropertyType.IsAssignableTo(typeof(bool)))
            {
                Error($"Property {MemberName} is not a bool value or indexer, a list of bools, or a bool ModdedOption.");
                return null;
            }

            var indexParams = vProperty.GetIndexParameters();

            if (indexParams.Length > 1)
            {
                Error($"Indexer {MemberName} has too many parameters.");
                return null;
            }

            if (isListBool && indexParams.Length != 0)
            {
                Error($"Property {MemberName} cannot be an indexer of lists of bools.");
                return null;
            }

            if (indexParams.Length == 1)
            {
                if (indexParams[0].ParameterType != typeof(int))
                {
                    Error($"Indexer {MemberName}'s parameter is not an int.");
                    return null;
                }

                hasIndexParam = true;
            }

            targetMethod = vProperty.GetGetMethod(true);

            if (targetMethod is null)
            {
                Error($"Property {MemberName} does not have a getter.");
                return null;
            }
        }
        else if (member is MethodInfo vMethod)
        {
            if (vMethod.ReturnType != typeof(bool))
            {
                Error($"Method {MemberName} does not return a bool.");
                return null;
            }

            var paramList = vMethod.GetParameters();

            if (paramList.Length > 1)
            {
                Error($"Method {MemberName} has too many parameters.");
                return null;
            }

            if (paramList.Length == 1)
            {
                if (paramList[0].ParameterType != typeof(int))
                {
                    Error($"Method {MemberName}'s parameter is not an int.");
                    return null;
                }

                hasIndexParam = true;
            }

            targetMethod = vMethod;
        }
        else if (member is FieldInfo field)
        {
            if (field.FieldType.IsAssignableTo(typeof(IList<bool>)))
            {
                postProcess = il =>
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, ListBoolIndexerGetter);
                };
            }
            else if (field.FieldType != typeof(bool))
            {
                Error($"Field {MemberName} is not a bool or a list of bools.");
                return null;
            }

            var dynamicMethod = new DynamicMethod($"GetListVisibility_{type.Name}_{field.Name}", typeof(bool), [typeof(int)], type, true);
            var il = dynamicMethod.GetILGenerator();

            if (instanceGetter != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, instanceGetter);
                il.Emit(OpCodes.Ldfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, field);
            }

            postProcess?.Invoke(il);

            il.Emit(OpCodes.Ret);

            var func = dynamicMethod.CreateDelegate<Func<int, bool>>();
            return index => () => func(index);
        }
        else
        {
            Error($"Member {MemberName} of {type.FullName} is not valid for Visibility function.");
            return null;
        }

        return CompileDynamicListDelegate(type, targetMethod, instanceGetter, typeof(bool), Type.EmptyTypes, typeof(Func<bool>), "ListVisibility", hasIndexParam, postProcess, postProcessList);
    }
}

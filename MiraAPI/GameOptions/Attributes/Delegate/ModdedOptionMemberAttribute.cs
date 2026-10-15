using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Base attribute for resolving and compiling option delegate members (<see cref="IModdedOption.Visible"/>, <see cref="IModdedOption.Getter"/>, <see cref="IModdedOption.Setter"/>, and <see cref="IModdedOption.ChangedEvent"/>).
/// </summary>
/// <param name="memberName">The name of the target member, or <see langword="null"/> to use the property name with <paramref name="defaultSuffix"/>.</param>
/// <param name="holderType">The type containing the target member, or <see langword="null"/> to use the declaring <see cref="AbstractOptionGroup"/> type.</param>
/// <param name="defaultSuffix">The default suffix appended to the property name when <paramref name="memberName"/> is not specified.</param>
[AttributeUsage(AttributeTargets.Property)]
public abstract class ModdedOptionMemberAttribute(string? memberName, Type? holderType, string defaultSuffix) : Attribute
{
    /// <summary>
    /// Gets the resolved name of the target member.
    /// </summary>
    protected string? MemberName { get; private set; } = memberName;

    /// <summary>
    /// Resolves the target <see cref="MemberInfo"/> (property or method) and its associated singleton instance getter, if applicable.
    /// </summary>
    /// <param name="group">The option group instance the property belongs to.</param>
    /// <param name="property">The option property decorated with this attribute.</param>
    /// <param name="type">When this method returns, contains the resolved <see cref="Type"/> expected to hold the target member.</param>
    /// <param name="instanceGetter">
    /// When this method returns, contains the <see cref="OptionGroupSingleton{T}.Instance"/> getter <see cref="MethodInfo"/>
    /// if the resolved member is an instance member on an <see cref="AbstractOptionGroup"/>; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>The resolved <see cref="MemberInfo"/> if found; otherwise, <see langword="null"/>.</returns>
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "It is safe.")]
    protected MemberInfo? ResolveMember(AbstractOptionGroup group, PropertyInfo property, out Type type, out MethodInfo? instanceGetter)
    {
        var groupType = group.GetType();
        var flags = BindingFlags.Static | BindingFlags.Public;

        type = holderType ?? groupType;
        MemberName ??= $"{property.Name}{defaultSuffix}";

        if (type.IsAssignableTo(typeof(AbstractOptionGroup)))
        {
            flags |= BindingFlags.Instance;

            if (type == groupType)
                flags |= BindingFlags.NonPublic;

            instanceGetter = typeof(OptionGroupSingleton<>)
                .MakeGenericType(type)
                .GetProperty(nameof(OptionGroupSingleton<>.Instance))!
                .GetGetMethod();
        }
        else
        {
            instanceGetter = null;
        }

        var member = type.GetMember(MemberName, flags).FirstOrDefault();

        if (instanceGetter != null)
        {
            var method = (member as PropertyInfo)?.GetGetMethod(true) ?? member as MethodInfo;
            if (method is { IsStatic: true })
                instanceGetter = null;
        }

        return member;
    }

    /// <summary>
    /// Resolves the target <see cref="MethodInfo"/> and its associated singleton instance getter, if applicable.
    /// </summary>
    /// <param name="group">The option group instance the property belongs to.</param>
    /// <param name="property">The option property decorated with this attribute.</param>
    /// <param name="type">When this method returns, contains the resolved <see cref="Type"/> expected to hold the target method.</param>
    /// <param name="instanceGetter">
    /// When this method returns, contains the <see cref="OptionGroupSingleton{T}.Instance"/> getter <see cref="MethodInfo"/>
    /// if the resolved method is an instance method on an <see cref="AbstractOptionGroup"/>; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>The resolved <see cref="MethodInfo"/> if found; otherwise, <see langword="null"/>.</returns>
    protected MethodInfo? ResolveMethod(AbstractOptionGroup group, PropertyInfo property, out Type type, out MethodInfo? instanceGetter)
    {
        return ResolveMember(group, property, out type, out instanceGetter) as MethodInfo;
    }

    /// <summary>
    /// Compiles a <see cref="DynamicMethod"/> using IL generation to invoke the resolved static or instance <paramref name="method"/>.
    /// </summary>
    /// <param name="ownerType">The <see cref="Type"/> that owns the generated <see cref="DynamicMethod"/> and holds the target method.</param>
    /// <param name="method">The target <see cref="MethodInfo"/> to invoke.</param>
    /// <param name="instanceGetter">
    /// The static getter <see cref="MethodInfo"/> used to push the target instance onto the evaluation stack for instance methods,
    /// or <see langword="null"/> if <paramref name="method"/> is static.
    /// </param>
    /// <param name="returnType">The return <see cref="Type"/> of the generated delegate (or <see cref="void"/> for actions).</param>
    /// <param name="paramTypes">The parameter types accepted by the generated delegate.</param>
    /// <param name="delegateType">The concrete delegate <see cref="Type"/> to create (e.g., <see cref="Func{T, TResult}"/> or <see cref="Action{T}"/>).</param>
    /// <param name="suffix">A descriptive suffix appended to the generated <see cref="DynamicMethod"/> name for debugging and stack traces.</param>
    /// <param name="forwardParameters">Whether to push the delegate parameters onto the stack before invoking <paramref name="method"/>.</param>
    /// <param name="postProcessCall">An optional callback to emit additional IL instructions before returning.</param>
    /// <returns>A compiled <see cref="Delegate"/> of type <paramref name="delegateType"/> bound to the generated IL method.</returns>
    protected static Delegate CompileDynamicDelegate(
        Type ownerType,
        MethodInfo method,
        MethodInfo? instanceGetter,
        Type returnType,
        Type[] paramTypes,
        Type delegateType,
        string suffix,
        bool forwardParameters = true,
        Action<ILGenerator>? postProcessCall = null)
    {
        var dm = new DynamicMethod($"{ownerType.Name}_{method.Name}_{suffix}", returnType, paramTypes, ownerType, true);
        var il = dm.GetILGenerator();

        if (!method.IsStatic && instanceGetter != null)
            il.Emit(OpCodes.Call, instanceGetter);

        if (forwardParameters)
        {
            for (byte i = 0; i < paramTypes.Length; i++)
            {
                if (i == 0)
                    il.Emit(OpCodes.Ldarg_0);
                else if (i == 1)
                    il.Emit(OpCodes.Ldarg_1);
                else if (i == 2)
                    il.Emit(OpCodes.Ldarg_2);
                else if (i == 3)
                    il.Emit(OpCodes.Ldarg_3);
                else
                    il.Emit(OpCodes.Ldarg_S, i);
            }
        }

        var callOp = method.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
        il.Emit(callOp, method);

        postProcessCall?.Invoke(il);

        il.Emit(OpCodes.Ret);

        return dm.CreateDelegate(delegateType);
    }

    /// <summary>
    /// Compiles a factory function that provides a <see cref="Delegate"/> for each index in an option list.
    /// If <paramref name="hasIndexParameter"/> is <see langword="false"/> and <paramref name="postProcessListCall"/> is <see langword="null"/>,
    /// a single <see cref="Delegate"/> is compiled once and reused for all indices.
    /// </summary>
    /// <param name="ownerType">The <see cref="Type"/> that owns the generated <see cref="DynamicMethod"/> and holds the target method.</param>
    /// <param name="method">The target <see cref="MethodInfo"/> to invoke.</param>
    /// <param name="instanceGetter">
    /// The static getter <see cref="MethodInfo"/> used to push the target instance onto the evaluation stack for instance methods,
    /// or <see langword="null"/> if <paramref name="method"/> is static.
    /// </param>
    /// <param name="returnType">The return <see cref="Type"/> of the generated delegate (or <see cref="void"/> for actions).</param>
    /// <param name="baseParamTypes">The parameter types accepted by the generated delegate.</param>
    /// <param name="delegateType">The concrete delegate <see cref="Type"/> to create.</param>
    /// <param name="suffix">A descriptive suffix appended to the generated <see cref="DynamicMethod"/> name for debugging and stack traces.</param>
    /// <param name="hasIndexParameter">Whether the target <paramref name="method"/> takes the list <see langword="int"/> index as its final parameter.</param>
    /// <param name="postProcessCall">An optional index-independent callback to emit additional IL instructions after <paramref name="method"/> is called.</param>
    /// <param name="postProcessListCall">An optional index-dependent callback to emit additional IL instructions after <paramref name="method"/> is called.</param>
    /// <returns>A factory function that returns a compiled <see cref="Delegate"/> of type <paramref name="delegateType"/> for a given list index.</returns>
    protected static Func<int, Delegate> CompileDynamicListDelegate(
        Type ownerType,
        MethodInfo method,
        MethodInfo? instanceGetter,
        Type returnType,
        Type[] baseParamTypes,
        Type delegateType,
        string suffix,
        bool hasIndexParameter,
        Action<ILGenerator>? postProcessCall = null,
        Action<ILGenerator, int>? postProcessListCall = null)
    {
        if (!hasIndexParameter && postProcessListCall is null)
        {
            var sharedDelegate = CompileDynamicDelegate(
                ownerType,
                method,
                instanceGetter,
                returnType,
                baseParamTypes,
                delegateType,
                suffix,
                forwardParameters: baseParamTypes.Length > 0,
                postProcessCall: postProcessCall);

            return _ => sharedDelegate;
        }

        return index =>
        {
            var dm = new DynamicMethod($"{ownerType.Name}_{method.Name}_{suffix}_{index}", returnType, baseParamTypes, ownerType, true);
            var il = dm.GetILGenerator();

            if (!method.IsStatic && instanceGetter != null)
                il.Emit(OpCodes.Call, instanceGetter);

            for (byte i = 0; i < baseParamTypes.Length; i++)
            {
                if (i == 0) il.Emit(OpCodes.Ldarg_0);
                else if (i == 1) il.Emit(OpCodes.Ldarg_1);
                else if (i == 2) il.Emit(OpCodes.Ldarg_2);
                else if (i == 3) il.Emit(OpCodes.Ldarg_3);
                else il.Emit(OpCodes.Ldarg_S, i);
            }

            if (hasIndexParameter)
                il.Emit(OpCodes.Ldc_I4, index);

            var callOp = method.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
            il.Emit(callOp, method);

            postProcessCall?.Invoke(il);
            postProcessListCall?.Invoke(il, index);

            il.Emit(OpCodes.Ret);

            return dm.CreateDelegate(delegateType);
        };
    }
}

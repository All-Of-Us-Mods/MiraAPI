using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MiraAPI.Utilities;

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
    private static readonly Dictionary<MethodInfo, Delegate> DelegateCache = [];

    /// <summary>
    /// A helper class to bind a specific list index to a dynamically compiled delegate.
    /// </summary>
    /// <param name="index">The index to bind.</param>
    public sealed class DelegateIndexClosure(int index)
    {
        /// <summary>
        /// The bound index.
        /// </summary>
        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "No need to be private.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
        public readonly int Index = index;
    }

    /// <summary>
    /// Exposes the index field so child attributes can emit instructions to read it.
    /// </summary>
    protected static readonly FieldInfo IndexClosureField = typeof(DelegateIndexClosure).GetField(nameof(DelegateIndexClosure.Index))!;

    /// <summary>
    /// Gets the resolved name of the target member.
    /// </summary>
    protected string? MemberName { get; private set; } = memberName;

    /// <summary>
    /// Validates that a method matches the expected return type and parameter types.
    /// </summary>
    /// <param name="method">The method to validate.</param>
    /// <param name="expectedReturnType">The expected return type of the method.</param>
    /// <param name="expectedParamTypes">The expected types of the parameters of the method.</param>
    /// <returns><c>true</c> if the method is valid; <c>false</c> otherwise.</returns>
    protected bool ValidateSignature(MethodInfo method, Type expectedReturnType, params Type[] expectedParamTypes)
    {
        if (method.ReturnType != expectedReturnType)
        {
            Error($"{MemberName}'s return type must be {expectedReturnType.Name}.");
            return false;
        }

        var parameters = method.GetParameters();

        if (parameters.Length != expectedParamTypes.Length)
        {
            Error($"{MemberName} must have exactly {expectedParamTypes.Length} parameter(s).");
            return false;
        }

        for (var i = 0; i < expectedParamTypes.Length; i++)
        {
            if (parameters[i].ParameterType == expectedParamTypes[i])
                continue;

            Error($"{MemberName}'s parameter at index {i} must be {expectedParamTypes[i].Name}.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a list method matches the expected return type, base parameter types, and an optional integer index.
    /// </summary>
    /// <param name="method">The method to validate.</param>
    /// <param name="expectedReturnType">The expected return type of the method.</param>
    /// <param name="hasIndex">When the method returns, a flag that indicates if the method has an index parameter.</param>
    /// <param name="expectedParamTypes">The expected types of the parameters of the method.</param>
    /// <returns><c>true</c> if the method is valid; <c>false</c> otherwise.</returns>
    protected bool ValidateListSignature(MethodInfo method, Type expectedReturnType, out bool hasIndex, params Type[] expectedParamTypes)
    {
        hasIndex = false;

        if (method.ReturnType != expectedReturnType)
        {
            Error($"{MemberName}'s return type must be {expectedReturnType.Name}.");
            return false;
        }

        var parameters = method.GetParameters();

        if (parameters.Length < expectedParamTypes.Length || parameters.Length > expectedParamTypes.Length + 1)
        {
            Error($"{MemberName} must have {expectedParamTypes.Length} parameter(s), optionally with an extra trailing int parameter.");
            return false;
        }

        for (var i = 0; i < expectedParamTypes.Length; i++)
        {
            if (parameters[i].ParameterType == expectedParamTypes[i])
                continue;

            Error($"{MemberName}'s parameter at index {i} must be {expectedParamTypes[i].Name}.");
            return false;
        }

        if (parameters.Length == expectedParamTypes.Length + 1)
        {
            if (parameters[^1].ParameterType != typeof(int))
            {
                Error($"{MemberName}'s final parameter must be an int (the list index).");
                return false;
            }

            hasIndex = true;
        }

        return true;
    }

    /// <summary>
    /// Resolves the target <see cref="MemberInfo"/> (property, field or method) and its associated singleton instance getter, if applicable.
    /// </summary>
    /// <param name="group">The option group instance the property belongs to.</param>
    /// <param name="property">The option property decorated with this attribute.</param>
    /// <param name="type">When this method returns, contains the resolved <see cref="Type"/> expected to hold the target member.</param>
    /// <param name="instanceGetter">
    /// When this method returns, contains the <see cref="OptionGroupSingleton{T}.Instance"/> getter <see cref="MethodInfo"/>
    /// if the resolved member is an instance member on an <see cref="AbstractOptionGroup"/>; otherwise, <see langword="null"/>.
    /// </param>
    /// <param name="notFound">When this method returns, contains a flag that indicates if the member was not found, as null can be returned from other circumstances too.</param>
    /// <returns>The resolved <see cref="MemberInfo"/> if found; otherwise, <see langword="null"/>.</returns>
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "It is safe.")]
    protected MemberInfo? ResolveMember(AbstractOptionGroup group, PropertyInfo property, out Type type, out MethodInfo? instanceGetter, out bool notFound)
    {
        var groupType = group.GetType();
        var flags = BindingFlags.Static | BindingFlags.Public;

        MemberName ??= property.Name + defaultSuffix;

        type = holderType ?? groupType;
        instanceGetter = null;

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
        else if (!type.IsStatic())
        {
            flags |= BindingFlags.Instance;

            var canSearchPrivates = type.GetCustomAttribute<SearchPrivateAttribute>() != null;

            if (canSearchPrivates)
                flags |= BindingFlags.NonPublic;

            var getterFlags = BindingFlags.Static | BindingFlags.Public | (canSearchPrivates ? BindingFlags.NonPublic : BindingFlags.Default);

            foreach (var method in type.GetMethods(getterFlags))
            {
                if (method.GetParameters().Length > 0 || (method.GetCustomAttribute<InstanceGetterAttribute>() == null && method.Name != "GetInstance"))
                    continue;

                instanceGetter = method;
                break;
            }

            if (instanceGetter == null)
            {
                foreach (var prop in type.GetProperties(getterFlags))
                {
                    if (prop.GetCustomAttribute<InstanceGetterAttribute>() == null && prop.Name != "Instance")
                        continue;

                    var getMethod = prop.GetGetMethod(canSearchPrivates);

                    if (getMethod == null)
                        continue;

                    instanceGetter = getMethod;
                    break;
                }
            }

            if (instanceGetter == null)
            {
                Error($"No valid static instance getter (method GetInstance or property Instance) found in instanced type {type.Name}. Try renaming the method/property, or decorate it with InstanceGetterAttribute.");
                notFound = false;
                return null;
            }

            if (!instanceGetter.ReturnType.IsAssignableTo(type))
            {
                Error($"Found instance getter {instanceGetter.Name}, but the return type did not match what was expected ({type.Name}).");
                notFound = false;
                return null;
            }
        }
        else if (type.GetCustomAttribute<SearchPrivateAttribute>() != null)
        {
            flags |= BindingFlags.NonPublic;
        }

        var member = type.GetMember(MemberName, flags).FirstOrDefault();
        notFound = true;

        if (member == null)
            return null;

        notFound = false;

        if (instanceGetter == null)
            return member;

        bool isStatic;

        if (member is FieldInfo field)
        {
            isStatic = field.IsStatic;
        }
        else
        {
            var method = (member as PropertyInfo)?.GetGetMethod(true) ?? member as MethodInfo;
            isStatic = method is { IsStatic: true };
        }

        if (isStatic)
            instanceGetter = null;

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
    /// <param name="notFound">When this method returns, contains a flag that indicates if the member was not found, as null can be returned from other circumstances too.</param>
    /// <returns>The resolved <see cref="MethodInfo"/> if found; otherwise, <see langword="null"/>.</returns>
    protected MethodInfo? ResolveMethod(AbstractOptionGroup group, PropertyInfo property, out Type type, out MethodInfo? instanceGetter, out bool notFound)
    {
        return ResolveMember(group, property, out type, out instanceGetter, out notFound) as MethodInfo;
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
        if (postProcessCall == null && DelegateCache.TryGetValue(method, out var cachedDelegate))
            return cachedDelegate;

        var dm = new DynamicMethod($"{ownerType.Name}_{method.Name}_{suffix}", returnType, paramTypes, ownerType, true);
        var il = dm.GetILGenerator();

        if (!method.IsStatic && instanceGetter != null)
            il.Emit(OpCodes.Call, instanceGetter);

        if (forwardParameters)
        {
            for (byte i = 0; i < paramTypes.Length; i++)
            {
                var loadOp = i switch
                {
                    0 => OpCodes.Ldarg_0,
                    1 => OpCodes.Ldarg_1,
                    2 => OpCodes.Ldarg_2,
                    3 => OpCodes.Ldarg_3,
                    _ => OpCodes.Ldarg_S,
                };

                if (loadOp == OpCodes.Ldarg_S)
                    il.Emit(loadOp, i);
                else
                    il.Emit(loadOp);
            }
        }

        var callOp = method.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
        il.Emit(callOp, method);

        postProcessCall?.Invoke(il);

        il.Emit(OpCodes.Ret);

        var compiledDelegate = dm.CreateDelegate(delegateType);

        if (postProcessCall == null)
            DelegateCache[method] = compiledDelegate;

        return compiledDelegate;
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
        Action<ILGenerator>? postProcessListCall = null)
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
                baseParamTypes.Length > 0,
                postProcessCall);

            return _ => sharedDelegate;
        }

        var fullParamTypes = new Type[baseParamTypes.Length + 1];
        fullParamTypes[0] = typeof(DelegateIndexClosure);
        Array.Copy(baseParamTypes, 0, fullParamTypes, 1, baseParamTypes.Length);

        var dm = new DynamicMethod($"{ownerType.Name}_{method.Name}_{suffix}_List", returnType, fullParamTypes, ownerType, true);
        var il = dm.GetILGenerator();

        if (!method.IsStatic && instanceGetter != null)
            il.Emit(OpCodes.Call, instanceGetter);

        for (var i = 0; i < baseParamTypes.Length; i++)
        {
            var argPos = (byte)(i + 1);
            var loadOp = argPos switch
            {
                1 => OpCodes.Ldarg_1,
                2 => OpCodes.Ldarg_2,
                3 => OpCodes.Ldarg_3,
                _ => OpCodes.Ldarg_S,
            };

            if (loadOp == OpCodes.Ldarg_S)
                il.Emit(loadOp, argPos);
            else
                il.Emit(loadOp);
        }

        if (hasIndexParameter)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, IndexClosureField);
        }

        var callOp = method.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
        il.Emit(callOp, method);

        postProcessCall?.Invoke(il);
        postProcessListCall?.Invoke(il);

        il.Emit(OpCodes.Ret);

        return index => dm.CreateDelegate(delegateType, new DelegateIndexClosure(index));
    }
}

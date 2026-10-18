using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using MiraAPI.Utilities;

namespace MiraAPI.Modifiers;

/// <summary>
/// Factory for creating instances of a <see cref="BaseModifier"/>. More efficient than using reflection.
/// </summary>
public static class ModifierFactory
{
    private static readonly Dictionary<(Type, Type[]), Func<object[], BaseModifier>> ConstructorCache = [];

    private static Func<object[], BaseModifier> CreateConstructor(Type type, params object[] args)
    {
        var constructorInfo = type.GetBestConstructor(args) ?? throw new InvalidOperationException(
            $"Could not find a constructor for type {type} with the specified arguments.");

        var parameters = constructorInfo.GetParameters();

        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var constructorParams = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var paramAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var convertedParam = Expression.Convert(paramAccess, paramType);
            constructorParams[i] = convertedParam;
        }

        var newExpr = Expression.New(constructorInfo, constructorParams);
        var lambda = Expression.Lambda<Func<object[], BaseModifier>>(newExpr, argsParam);

        return lambda.Compile();
    }

    /// <summary>
    /// Creates a <see cref="BaseModifier"/> with the specified type and arguments.
    /// </summary>
    /// <param name="type">Modifier type.</param>
    /// <param name="args">Arguments.</param>
    /// <returns>An instance of the <see cref="BaseModifier"/>.</returns>
    public static BaseModifier CreateInstance(Type type, params object[] args)
    {
        var argTypes = args.Select(arg => arg.GetType()).ToArray();
        var key = (type, argTypes);

        if (!ConstructorCache.TryGetValue(key, out var constructor))
        {
            constructor = CreateConstructor(type, args);
            ConstructorCache[key] = constructor;
        }

        return constructor(args);
    }
}

/// <summary>
/// Factory for creating instances of <typeparamref name="T"/>. More efficient than using reflection.
/// </summary>
/// <typeparam name="T">The <see cref="BaseModifier"/> type.</typeparam>
public static class ModifierFactory<T>
    where T : BaseModifier
{
    private static readonly Func<object[], T> Constructor = CreateConstructor();

    private static Func<object[], T> CreateConstructor(params object[] args)
    {
        var constructorInfo = typeof(T).GetBestConstructor(args) ?? throw new InvalidOperationException(
            $"Could not find a constructor for type {typeof(T)} with the specified arguments.");

        var parameters = constructorInfo.GetParameters();

        var argsParam = Expression.Parameter(typeof(object[]), "args");
        var constructorParams = new Expression[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            var paramAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
            var convertedParam = Expression.Convert(paramAccess, paramType);
            constructorParams[i] = convertedParam;
        }

        var newExpr = Expression.New(constructorInfo, constructorParams);
        var lambda = Expression.Lambda<Func<object[], T>>(newExpr, argsParam);

        return lambda.Compile();
    }

    /// <summary>
    /// Creates an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="args">Parameters for the constructor.</param>
    /// <returns>An instance of <typeparamref name="T"/>.</returns>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "This is a factory class.")]
    public static T CreateInstance(params object[] args)
    {
        return Constructor(args);
    }
}

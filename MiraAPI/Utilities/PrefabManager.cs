using Il2CppInterop.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiraAPI.Utilities;

// Original code from: https://github.com/D1GQ/AmongUsPrefabsAPI

/// <summary>
/// A static class for handling prefab management within Among Us.
/// This class provides functionality to load, cache, retrieve, and remove prefabs dynamically.
/// It ensures that prefabs can be instantiated, stored, and managed efficiently without requiring Unity Editor access.
/// About any Among Us MonoBehavior/Component prefab can be loaded from the game.
/// Note: This can only grab game prefabs that are currently in memory. Prefabs must have been loaded by the game at runtime to be accessible.
/// The class supports indexing to differentiate between multiple prefabs of the same component type.
/// Use the index parameter only when there is more than one prefab for a specific component type.
/// If there is only one prefab for a component, the default index (0) should be used.
/// </summary>
public static class PrefabManager
{
    private static readonly HashSet<string> CachedTypes = [];
    private static readonly Dictionary<string, GameObject?> CachedPrefabs = [];
    private static readonly Dictionary<string, GameObject?> TempPrefabs = [];

    private static T? LoadPrefab<T>(Transform? parent, int index, int cacheType) where T : Component
    {
        var il2cppType = Il2CppType.Of<T>();
        var component = Resources.FindObjectsOfTypeAll(il2cppType)
            .Where(com => com.GetIl2CppType() == il2cppType && !com.Cast<Component>().gameObject.scene.isLoaded)
            .ElementAtOrDefault(index)
            ?.Cast<T>();

        if (component == null) return null;

        var instance = UnityEngine.Object.Instantiate(component.gameObject, parent);
        instance.name = instance.name.Replace("(Clone)", string.Empty);

        string typeName = typeof(T).FullName + $"-{index}";

        if (cacheType == 1)
        {
            instance.name += "(Prefab)";
            CachedTypes.Add(typeName);
            CachedPrefabs[typeName] = instance.gameObject;
            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);
        }
        else if (cacheType == 2)
        {
            instance.name += "(Temp)";
            TempPrefabs[typeName] = instance;
        }

        return instance.GetComponent<T>();
    }

    /// <summary>
    /// Copies a prefab of type <typeparamref name="T"/> without caching it.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="parent">The parent Transform to attach the instantiated prefab to (optional).</param>
    /// <param name="index">The index of the prefab to load, if there are multiple prefabs for the same component type. Default is 0.</param>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    public static T? CopyPrefab<T>(Transform? parent, int index = 0) where T : Component
    {
        return LoadPrefab<T>(parent, index, 0);
    }

    /// <summary>
    /// Copies a prefab of type <typeparamref name="T"/> without caching it.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="index">The index of the prefab to load, if there are multiple prefabs for the same component type. Default is 0.</param>
    /// <param name="parent">The parent Transform to attach the instantiated prefab to (optional).</param>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    public static T? CopyPrefab<T>(int index, Transform? parent = null) where T : Component => CopyPrefab<T>(parent, index);

    /// <summary>
    /// Copies a prefab of type <typeparamref name="T"/> without caching it.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    public static T? CopyPrefab<T>() where T : Component => CopyPrefab<T>(null, 0);

    /// <summary>
    /// Retrieves a temporarily cached prefab of type <typeparamref name="T"/> from the cache.
    /// If the prefab is not already cached, it will be loaded and temporarily cached for future use.
    /// Note: Temporarily cached prefabs can be destroyed on load, unlike cached prefabs.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="index">The index of the prefab to load, if there are multiple prefabs for the same component type. Default is 0.</param>
    /// <returns>An instance of the requested prefab component if found, otherwise null.</returns>
    public static T? GetTempPrefab<T>(int index = 0) where T : Component
    {
        string typeName = typeof(T).FullName + $"-{index}";

        if (TempPrefabs.TryGetValue(typeName, out var obj) && obj != null)
        {
            return obj.GetComponent<T>();
        }

        return LoadPrefab<T>(null, index, 2);
    }

    /// <summary>
    /// Retrieves a cached prefab of type <typeparamref name="T"/>.
    /// If the prefab is not already cached, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="index">The index of the prefab to load, if there are multiple prefabs for the same component type. Default is 0.</param>
    /// <returns>The cached instance of the requested prefab.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the requested prefab is not cached.</exception>
    public static T? GetCachedPrefab<T>(int index = 0) where T : Component
    {
        string typeName = typeof(T).FullName + $"-{index}";

        if (!CachedPrefabs.TryGetValue(typeName, out var obj) || obj == null)
            throw new InvalidOperationException($"Unable to get a prefab of type {typeof(T).Name} that hasn't been cached!");

        return obj.GetComponent<T>();
    }

    /// <summary>
    /// Caches a prefab of type <typeparamref name="T"/> to allow retrieval later,
    /// placing it into DontDestroyOnLoad to persist across scene changes.
    /// If the prefab is already cached, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="index">The index of the prefab to cache, if there are multiple prefabs for the same component type. Default is 0.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the prefab is already cached.
    /// </exception>
    public static void CachePrefab<T>(int index = 0) where T : Component
    {
        string typeName = typeof(T).FullName + $"-{index}";

        if (CachedTypes.Contains(typeName))
            throw new InvalidOperationException("Unable to cache a prefab that's already been cached!");

        LoadPrefab<T>(null, index, 1);
    }

    /// <summary>
    /// Removes a cached prefab of type <typeparamref name="T"/>.
    /// If the prefab is not cached, an exception is thrown.
    /// </summary>
    /// <typeparam name="T">The component type of the prefab.</typeparam>
    /// <param name="index">The index of the prefab to uncache, if there are multiple prefabs for the same component type. Default is 0.</param>
    /// <exception cref="InvalidOperationException">Thrown if the prefab is not cached.</exception>
    public static void UncachePrefab<T>(int index = 0) where T : Component
    {
        string typeName = typeof(T).FullName + $"-{index}";

        if (!CachedPrefabs.TryGetValue(typeName, out var obj))
            throw new InvalidOperationException($"Unable to uncache a prefab of type {typeof(T).Name} that hasn't been cached!");

        CachedTypes.Remove(typeName);
        CachedPrefabs.Remove(typeName);

        if (obj) UnityEngine.Object.Destroy(obj);
    }

    /// <summary>
    /// Clears all cached prefabs, removing their references and destroying their instances.
    /// </summary>
    public static void UncacheAll()
    {
        foreach (var obj in CachedPrefabs.Values)
        {
            if (obj) UnityEngine.Object.Destroy(obj);
        }

        CachedPrefabs.Clear();
        CachedTypes.Clear();
    }
}
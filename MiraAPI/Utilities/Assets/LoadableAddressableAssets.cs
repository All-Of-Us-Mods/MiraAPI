using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading groups of assets from ALL addressable locations.
/// </summary>
/// <typeparam name="T">The type of the asset to be loaded.</typeparam>
public class LoadableAddressableAssets<T>(string key) where T : UnityEngine.Object
{
    private readonly Action<LoadableAddressableAssets<T>>? _gcAction;

    /// <summary>
    /// Gets or sets reference to the loaded asset. Intended to be used for caching purposes.
    /// </summary>
    protected List<T>? LoadedAssets { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableAddressableAssets{T}"/> class, with added garbageCollector logic,
    /// to help with <see href="https://github.com/BepInEx/Il2CppInterop/issues/40"/>.
    /// </summary>
    /// <param name="key">The key of the assets.</param>
    /// <param name="garbageCollection">A lambda that allows GCHandle.Alloc calls without garbage collection interferences.</param>
    public LoadableAddressableAssets(string key, Action<LoadableAddressableAssets<T>> garbageCollection) : this(key)
    {
        _gcAction = garbageCollection;
    }

    /// <summary>
    /// Loads the asset from addressables.
    /// </summary>
    /// <returns>The asset.</returns>
    /// <exception cref="Exception">The asset did not load properly.</exception>
    public ReadOnlyCollection<T> LoadAssets()
    {
        if (LoadedAssets != null)
        {
            return LoadedAssets.AsReadOnly();
        }

        var locations = Addressables.LoadResourceLocationsAsync(key).WaitForCompletion();

        if (!GC.TryStartNoGCRegion(4096, true)) Error("Could not start NoGCRegion of size 4kb, there is the possibility of injected unmanaged classes being garbage collected as per BepInEx/Il2CppInterop/issues/40");

        var assetsIList = Addressables.LoadAssetsAsync<T>(locations, null, false).WaitForCompletion();
        if (assetsIList == null)
        {
            GC.EndNoGCRegion();
            throw new InvalidOperationException($"INVALID ASSET/s: {key}");
        }

        var assetsList = new CppCollections.List<T>(assetsIList.Pointer);
        LoadedAssets = assetsList.ToArray().ToList();

        _gcAction?.Invoke(this);
        GC.EndNoGCRegion();

        return LoadedAssets.AsReadOnly();
    }
}

using System;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading an asset from ALL addressable locations.
/// </summary>
/// <param name="uid">The guid of the asset.</param>
/// <typeparam name="T">The type of the asset to be loaded.</typeparam>
public class LoadableAddressableAsset<T>(string uid) : LoadableAsset<T>
    where T : UnityEngine.Object
{
    private readonly Action<LoadableAddressableAsset<T>>? _gcAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableAddressableAsset{T}"/> class, with added garbageCollector logic,
    /// to help with <see href="https://github.com/BepInEx/Il2CppInterop/issues/40"/>.
    /// </summary>
    /// <param name="uid">The guid of the asset.</param>
    /// <param name="garbageCollection">A lambda that allows GCHandle.Alloc calls without garbage collection interferences.</param>
    public LoadableAddressableAsset(string uid, Action<LoadableAddressableAsset<T>> garbageCollection)
        : this(uid)
    {
        _gcAction = garbageCollection;
    }

    /// <summary>
    /// Loads the asset from addressables.
    /// </summary>
    /// <returns>The asset.</returns>
    /// <exception cref="Exception">The asset did not load properly.</exception>
    public override T LoadAsset()
    {
        if (LoadedAsset != null)
        {
            return LoadedAsset;
        }

        var remoteReference = new AssetReference(uid);

        if (!GC.TryStartNoGCRegion(4096, true)) Error("Could not start NoGCRegion of size 4kb, there is the possibility of injected unmanaged classes being garbage collected as per BepInEx/Il2CppInterop/issues/40");

        LoadedAsset = remoteReference.LoadAssetAsync<T>().WaitForCompletion();
        if (LoadedAsset == null)
        {
            GC.EndNoGCRegion();
            throw new InvalidOperationException($"INVALID ASSET: {uid}");
        }

        _gcAction?.Invoke(this);
        GC.EndNoGCRegion();

        return LoadedAsset;
    }
}

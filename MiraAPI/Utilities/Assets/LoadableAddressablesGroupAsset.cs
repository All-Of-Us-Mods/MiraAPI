using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading multiple assets from the addressables api.
/// </summary>
/// <param name="locations">A list of IResourceLocations that locate the assets.</param>
/// <typeparam name="T">The type of the assets to be loaded.</typeparam>
public class LoadableAddressableGroupAsset<T>(Il2CppSystem.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> locations) where T : UnityEngine.Object
{
    /// <summary>
    /// Gets or sets a reference to the loaded assets. Intended to be used for caching purposes.
    /// </summary>
    protected List<T> LoadedAsset { get; set; } = [];

    /// <summary>
    /// Loads the assets from the addressables api.
    /// </summary>
    /// <returns>The assets.</returns>
    /// <exception cref="Exception">The asset did not load properly.</exception>
    public List<T> LoadAsset()
    {
        if (LoadedAsset.Count != 0)
        {
            return LoadedAsset;
        }

        var assets = Addressables.LoadAssetsAsync<T>(locations, null, false).WaitForCompletion();
        var array = new Il2CppSystem.Collections.Generic.List<T>(assets.Pointer);
        LoadedAsset = [..array.ToArray()];

        if (LoadedAsset is null || LoadedAsset.Count == 0)
        {
            throw new InvalidOperationException("INVALID ASSET LOCATION/s");
        }

        return LoadedAsset;
    }
}

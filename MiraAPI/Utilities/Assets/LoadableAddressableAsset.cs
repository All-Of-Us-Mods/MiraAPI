using System;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading assets from the addressables api.
/// </summary>
/// <param name="name">The name/identifier of the asset</param>
/// <typeparam name="T">The type of the asset to be loaded</typeparam>
public class LoadableAddressableAsset<T>(string name) : LoadableAsset<T> where T : UnityEngine.Object 
{
    /// <summary>
    /// Loads the asset from the addressables api.
    /// </summary>
    /// <returns>The asset</returns>
    /// <exception cref="Exception">The asset did not load properly.</exception>
    public override T LoadAsset()
    {
        if (LoadedAsset)
        {
            return LoadedAsset;
        }

        LoadedAsset = Addressables.LoadAssetAsync<T>(name).WaitForCompletion();

        if (!LoadedAsset)
        {
            throw new Exception($"INVALID ASSET: {name}");
        }

        return LoadedAsset;
    }
}
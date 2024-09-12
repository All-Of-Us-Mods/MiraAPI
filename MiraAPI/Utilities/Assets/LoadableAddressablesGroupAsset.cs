using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets;

public class LoadableAddressableGroupAsset<T>(Il2CppSystem.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> locations) where T : UnityEngine.Object
{
    protected List<T> LoadedAsset = new();
    public List<T> LoadAsset()
    {
        if (LoadedAsset.Count != 0)
        {
            return LoadedAsset;
        }

        var assets = Addressables.LoadAssetsAsync<T>(locations, null, false).WaitForCompletion();
        var array = new Il2CppSystem.Collections.Generic.List<T>(assets.Pointer);
        LoadedAsset = new List<T>(array.ToArray());

        if (LoadedAsset is null || LoadedAsset.Count == 0)
        {
            throw new Exception($"INVALID ASSET LOCATION/s");
        }

        return LoadedAsset;
    }
}
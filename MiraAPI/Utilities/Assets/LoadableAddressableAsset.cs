using System;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets;

public class LoadableAddressableAsset<T>(string name) : LoadableAsset<T> where T : UnityEngine.Object 
{
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
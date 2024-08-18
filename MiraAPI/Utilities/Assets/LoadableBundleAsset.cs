using System;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Utilities.Assets
{
    public class LoadableBundleAsset<T>(string name, AssetBundle bundle) : LoadableAsset<T> where T : UnityEngine.Object
    {
        public override T LoadAsset()
        {
            if (_loadedAsset != null)
            {
                return _loadedAsset;
            }

            return _loadedAsset = bundle.LoadAsset<T>(name);
            throw new Exception($"INVALID ASSET: {name}");
        }
    }
}

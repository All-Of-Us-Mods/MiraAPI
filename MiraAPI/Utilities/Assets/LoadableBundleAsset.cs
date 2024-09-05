using MiraAPI.PluginLoading;
using Reactor.Utilities.Extensions;
using System;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading assets from an asset bundle.
/// </summary>
/// <param name="name">The name of the asset.</param>
/// <param name="bundle">The AssetBundle that contains the asset.</param>
/// <typeparam name="T">The type of the asset to be loaded.</typeparam>
public class LoadableBundleAsset<T> : LoadableAsset<T> where T : UnityEngine.Object
{

    /// <summary>
    /// Gets the ID of this asset. Used for networking.
    /// </summary>
    public uint AssetId { get; internal set; }
    private string _name { get; }
    private AssetBundle _bundle { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableBundleAsset{T}"/> class.
    /// </summary>
    /// <param name="name">Name of the bundle.</param>
    /// <param name="bundle">Asset bundle.</param>
    public LoadableBundleAsset(string name, AssetBundle bundle)
    {
        AssetId = MiraPluginManager.Instance.GetNextAssetId();
        //MiraPluginManager.Instance.BundleAssets.Add((LoadableBundleAsset<Object>)this);

        _name = name;
        _bundle = bundle;
    }

    /// <summary>
    /// Loads the asset from the asset bundle.
    /// </summary>
    /// <returns>The asset.</returns>
    /// <exception cref="Exception">The asset did not load properly.</exception>
    public override T LoadAsset()
    {
        if (LoadedAsset != null)
        {
            return LoadedAsset;
        }

        LoadedAsset = _bundle.LoadAsset<T>(_name);

        if (LoadedAsset == null)
        {
            throw new InvalidOperationException($"INVALID ASSET: {_name}");
        }

        return LoadedAsset;
    }
}

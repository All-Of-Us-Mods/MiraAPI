using System;
using System.Linq;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading multiple assets from an <see cref="LoadableBundleSubAssetHolder"/>.
/// </summary>
/// <param name="name">The name of the asset.</param>
/// <param name="assetHolder">The <see cref="LoadableBundleSubAssetHolder"/> that contains the asset.</param>
public class LoadableBundleSubAsset(string name, LoadableBundleSubAssetHolder assetHolder) : LoadableAsset<Sprite>
{
    /// <summary>
    /// Loads the asset from the <see cref="LoadableBundleSubAssetHolder"/>.
    /// </summary>
    /// <returns>The asset.</returns>
    /// <exception cref="Exception">The asset did not load properly.</exception>
    public override Sprite LoadAsset()
    {
        if (LoadedAsset != null)
        {
            return LoadedAsset;
        }

        assetHolder.TryInit();

        var loadedAsset = assetHolder.SubSprites.FirstOrDefault(x => x.name == name);

        if (loadedAsset == null)
        {
            throw new InvalidOperationException($"INVALID ASSETS: {name}");
        }
        LoadedAsset = loadedAsset;
        return LoadedAsset;
    }
}

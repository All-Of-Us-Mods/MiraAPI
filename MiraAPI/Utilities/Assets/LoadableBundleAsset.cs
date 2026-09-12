using System;
using System.Diagnostics.CodeAnalysis;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading assets from an <see cref="AssetBundle"/>.
/// </summary>
/// <param name="name">The name of the asset.</param>
/// <param name="bundle">The <see cref="AssetBundle"/> that contains the asset.</param>
/// <typeparam name="T">The type of the asset to be loaded.</typeparam>
public class LoadableBundleAsset<T>(string name, AssetBundle bundle) : LoadableAsset<T> where T : UnityEngine.Object
{
    /// <summary>
    /// Loads the asset from the <see cref="AssetBundle"/>.
    /// </summary>
    /// <returns>The asset.</returns>
    /// <exception cref="Exception">The asset did not load properly.</exception>
    [SuppressMessage("Style", "IDE0029:Use coalesce expression", Justification = "Null coalescing bypasses Unity lifetime checks.")]
    public override T LoadAsset()
    {
        if (LoadedAsset != null)
        {
            return LoadedAsset;
        }

        LoadedAsset = bundle.LoadAsset<T>(name);

        return LoadedAsset == null ? throw new InvalidOperationException($"INVALID ASSET: {name}") : LoadedAsset;
    }
}

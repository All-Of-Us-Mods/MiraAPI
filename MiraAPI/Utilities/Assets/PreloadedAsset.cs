using System;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// An implementation for creating loadable assets from existing assets.
/// </summary>
/// <typeparam name="T">The type of the asset to be loaded.</typeparam>
/// <param name="asset">The asset being preloaded.</param>
public class PreloadedAsset<T>(T asset) : LoadableAsset<T>
    where T : UObject
{
    private readonly T _loadedAsset = asset ?? throw new ArgumentNullException(nameof(asset));

    /// <inheritdoc/>
    public override T LoadAsset()
    {
        return _loadedAsset;
    }
}

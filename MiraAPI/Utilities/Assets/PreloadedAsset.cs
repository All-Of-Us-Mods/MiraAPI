using System;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// An implementation for creating loadable assets from existing assets.
/// <inheritdoc />
/// </summary>
/// <inheritdoc cref="LoadableAsset{T}"/>
[Obsolete("Use LoadableAssetWrapper<T> instead. This will be removed in a future version.")]
public class PreloadedAsset<T>(T asset) : LoadableAsset<T>
    where T : UObject
{
    private readonly T _loadedAsset = asset ?? throw new ArgumentNullException(nameof(asset));

    /// <inheritdoc />
    public override T LoadAsset()
    {
        return _loadedAsset;
    }
}

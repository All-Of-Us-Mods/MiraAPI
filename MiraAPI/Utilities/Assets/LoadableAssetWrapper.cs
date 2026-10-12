using Reactor.Utilities.Extensions;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// Wrapper class to turn any <see cref="UnityEngine.Object"/> into a <see cref="LoadableAsset{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the asset to be loaded. Must be a subclass of <see cref="UnityEngine.Object"/>.</typeparam>
public class LoadableAssetWrapper<T> : LoadableAsset<T>
    where T : UnityEngine.Object
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableAssetWrapper{T}"/> class.
    /// </summary>
    /// <param name="asset">The asset to wrap. This should be a reference to a <see cref="UnityEngine.Object"/>.</param>
    public LoadableAssetWrapper(T asset)
    {
        LoadedAsset = asset.DontDestroy();
    }

    /// <inheritdoc />
    public override T LoadAsset()
    {
        return LoadedAsset ?? throw new System.InvalidOperationException("The asset could not be loaded!");
    }
}

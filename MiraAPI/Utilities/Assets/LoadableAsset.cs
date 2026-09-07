using Reactor.Utilities.Extensions;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// An abstract class that provides a simple pattern for loading assets.
/// Mira uses the <see cref="LoadableAsset{T}"/> pattern in various locations.
/// You can create your own implementation of this class to load assets in different ways.
/// </summary>
/// <typeparam name="T">The type of the asset to be loaded.</typeparam>
public abstract class LoadableAsset<T> where T : UnityEngine.Object
{
    /// <summary>
    /// Gets or sets reference to the loaded asset. Intended to be used for caching purposes.
    /// </summary>
    protected T? LoadedAsset { get; set; }

    /// <summary>
    /// Loads the asset from the source.
    /// </summary>
    /// <returns>The loaded asset.</returns>
    public abstract T LoadAsset();

    /// <summary>
    /// Converts the <see cref="LoadableAsset{T}"/> into its <typeparamref name="T"/> by loading the asset.
    /// </summary>
    /// <param name="loadable">The <see cref="LoadableAsset{T}"/> to get the asset from.</param>
    public static implicit operator T(LoadableAsset<T> loadable) => loadable.LoadAsset();

    /// <summary>
    /// Unloads an asset.
    /// </summary>
    /// <returns>True if the asset was unloaded, false otherwise.</returns>
    public virtual bool UnloadAsset()
    {
        if (LoadedAsset == null || !LoadedAsset)
        {
            return false;
        }

        LoadedAsset.DestroyImmediate();
        LoadedAsset = null;
        return true;
    }
}

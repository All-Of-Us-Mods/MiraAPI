namespace MiraAPI.Utilities.Assets;

/// <summary>
/// An abstract class that provides a simple pattern for loading assets.
/// Mira uses the <see cref="LoadableAsset{T}"/> pattern in various locations.
/// You can create your own implementation of this class to load assets in different ways.
/// </summary>
/// <typeparam name="T">The type of the asset to be loaded</typeparam>
public abstract class LoadableAsset<T> where T : UnityEngine.Object
{
    /// <summary>
    /// A reference to the loaded asset. Intended to be used for caching purposes.
    /// </summary>
    protected T LoadedAsset;
        
    /// <summary>
    /// The method that loads the asset.
    /// </summary>
    /// <returns>The loaded asset</returns>
    public abstract T LoadAsset();
}
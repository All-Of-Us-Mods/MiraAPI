using System;
using System.Reflection;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading assets from embedded resources.
/// </summary>
public class LoadableResourceAsset : LoadableAsset<Sprite>
{
    private readonly string _path;
    private readonly float _pixelsPerUnit;
    private readonly Assembly _assembly;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableResourceAsset"/> class with the provided resource path and the assembly containing the asset.
    /// </summary>
    /// <param name="path">The path to the embedded resource.</param>
    /// <param name="assembly">The <see cref="Assembly"/> containing the asset.</param>
    /// <param name="pixelsPerUnit">The pixels per unit for the loaded sprite.</param>
    public LoadableResourceAsset(string path, Assembly assembly, float pixelsPerUnit = 100f)
    {
        _path = path;
        _assembly = assembly;
        _pixelsPerUnit = pixelsPerUnit;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableResourceAsset"/> class with the provided resource path and the assembly containing the asset.
    /// </summary>
    /// <param name="path">The path to the embedded resource.</param>
    /// <param name="pixelsPerUnit">The pixels per unit for the loaded sprite.</param>
    [Obsolete("Pass typeof(YourPlugin).Assembly directly to the constructor for better performance.")]
    public LoadableResourceAsset(string path, float pixelsPerUnit = 100f)
    {
        _path = path;
        _pixelsPerUnit = pixelsPerUnit;
        _assembly = Assembly.GetCallingAssembly();
    }

    /// <inheritdoc />
    public override Sprite LoadAsset()
    {
        if (!LoadedAsset)
        {
            LoadedAsset = SpriteTools.LoadSpriteFromPath(_path, _assembly, _pixelsPerUnit);
        }

        return LoadedAsset!;
    }
}

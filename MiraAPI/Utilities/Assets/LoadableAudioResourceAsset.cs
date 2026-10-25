using System;
using System.Reflection;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading .WAV audio assets from the DLL's embedded resources.
/// </summary>
public class LoadableAudioResourceAsset : LoadableAsset<AudioClip>
{
    private readonly string _path;
    private readonly Assembly _assembly;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableAudioResourceAsset"/> class with the provided resource path and the assembly containing the asset.
    /// </summary>
    /// <param name="path">The path of the wave file.</param>
    /// <param name="assembly">The <see cref="Assembly"/> containing the asset.</param>
    public LoadableAudioResourceAsset(string path, Assembly assembly)
    {
        _path = path;
        _assembly = assembly;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableAudioResourceAsset"/> class with the provided resource path.
    /// </summary>
    /// <param name="path">The path of the wave file.</param>
    [Obsolete("Pass typeof(YourPlugin).Assembly directly to the constructor for better performance.")]
    public LoadableAudioResourceAsset(string path)
    {
        _path = path;
        _assembly = Assembly.GetCallingAssembly();
    }

    /// <inheritdoc />
    public override AudioClip LoadAsset()
    {
        if (!LoadedAsset)
        {
            LoadedAsset = AudioTools.LoadAudioFromResources(_path, _assembly);
        }

        return LoadedAsset!;
    }
}

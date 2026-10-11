using System.Reflection;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading .WAV audio assets from the DLL's embedded resources.
/// </summary>
/// <param name="path">The path of the wave file.</param>
public class LoadableAudioResourceAsset(string path) : LoadableAsset<AudioClip>
{
    private readonly Assembly _assembly = Assembly.GetCallingAssembly();

    /// <inheritdoc />
    public override AudioClip LoadAsset()
    {
        if (!LoadedAsset)
        {
            LoadedAsset = AudioTools.LoadAudioFromResources(path, _assembly);
        }

        return LoadedAsset!;
    }
}

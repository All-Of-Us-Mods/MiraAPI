using System.Reflection;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading assets from embedded resources.
/// </summary>
/// <param name="path">The path to the embedded resource.</param>
/// <param name="pixelsPerUnit">The pixels per unit for the loaded sprite.</param>
public class LoadableResourceAsset(string path, float pixelsPerUnit = 100f) : LoadableAsset<Sprite>
{
    private readonly Assembly _assembly = Assembly.GetCallingAssembly();

    /// <inheritdoc />
    public override Sprite LoadAsset()
    {
        if (!LoadedAsset)
        {
            LoadedAsset = SpriteTools.LoadSpriteFromPath(path, _assembly, pixelsPerUnit);
        }

        return LoadedAsset!;
    }
}

using System.Reflection;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading assets from embedded resources.
/// </summary>
/// <param name="path">The path to the embedded resource.</param>
public class LoadableResourceAsset(string path) : LoadableAsset<Sprite>
{
    private readonly Assembly _assembly = Assembly.GetCallingAssembly();

    /// <inheritdoc />
    public override Sprite LoadAsset()
    {
        if (LoadedAsset != null)
        {
            return LoadedAsset;
        }

        return LoadedAsset = SpriteTools.LoadSpriteFromPath(path, _assembly);
    }
}

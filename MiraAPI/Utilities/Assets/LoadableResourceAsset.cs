using System.Reflection;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

public class LoadableResourceAsset(string path) : LoadableAsset<Sprite>
{
    private readonly Assembly _assembly = Assembly.GetCallingAssembly();

    public override Sprite LoadAsset()
    {
        if (LoadedAsset)
        {
            return LoadedAsset;
        }

        try
        {
            return LoadedAsset = SpriteTools.LoadSpriteFromPath(path, _assembly);
        }
        catch
        {
            Logger<MiraApiPlugin>.Error($"Not loading, invalid asset: {path}");
            return null;
        }
    }
}
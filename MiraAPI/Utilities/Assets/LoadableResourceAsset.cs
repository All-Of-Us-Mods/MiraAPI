using UnityEngine;

namespace MiraAPI.Utilities.Assets;

public class LoadableResourceAsset(string path) : LoadableAsset<Sprite>
{
    public override Sprite LoadAsset()
    {
        if (LoadedAsset != null)
        {
            return LoadedAsset;
        }

        try
        {
            return LoadedAsset = SpriteTools.LoadSpriteFromPath(path);
        }
        catch
        {
            Debug.LogError($"Not loading, invalid asset: {path}");
            return null;
        }
    }
}
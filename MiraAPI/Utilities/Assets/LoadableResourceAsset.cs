using System;
using UnityEngine;

namespace MiraAPI.Utilities.Assets
{
    public class LoadableResourceAsset(string name, string resourcesFolder) : LoadableAsset<Sprite>
    {
        public override Sprite LoadAsset()
        {
            if (_loadedAsset != null)
            {
                return _loadedAsset;
            }

            return _loadedAsset = SpriteTools.LoadSpriteFromPath(resourcesFolder + name);
            throw new Exception($"INVALID ASSET: {name}");
        }
    }
}

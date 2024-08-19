using UnityEngine;

namespace MiraAPI.Utilities.Assets
{
    public class LoadableResourceAsset(string path) : LoadableAsset<Sprite>
    {
        public override Sprite LoadAsset()
        {
            if (_loadedAsset != null)
            {
                return _loadedAsset;
            }

            try
            {
                return _loadedAsset = SpriteTools.LoadSpriteFromPath(path);
            }
            catch
            {
                Debug.LogError($"Not loading, invalid asset: {path}");
                return null;
            }
        }
    }
}

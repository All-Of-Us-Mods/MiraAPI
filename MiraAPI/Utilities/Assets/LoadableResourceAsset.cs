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

            try
            {
                return _loadedAsset = SpriteTools.LoadSpriteFromPath(resourcesFolder + name);
            }
            catch
            {
                Debug.LogError($"Not loading, invalid asset: {name}");
                return null;
            }
        }
    }
}

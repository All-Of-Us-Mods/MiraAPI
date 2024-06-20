namespace MiraAPI.Utilities.Assets
{
    public abstract class LoadableAsset<T> where T : UnityEngine.Object
    {
        protected T _loadedAsset;
        public abstract T LoadAsset();
    }
}

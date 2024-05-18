using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI
{
    public interface IMiraConfig
    {
        public ModdedOptionTabSettings TabSettings { get; }
    }

    public struct ModdedOptionTabSettings(string title, LoadableAsset<Sprite> tabIcon)
    {
        public string Title { get; set; } = title;
        public LoadableAsset<Sprite> TabIcon { get; set; } = tabIcon;
    }
}

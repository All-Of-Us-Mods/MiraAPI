using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.GameOptions
{
    public interface IModOptions
    {
        LoadableAsset<Sprite> OptionsTabIcon { get; }
    }
}

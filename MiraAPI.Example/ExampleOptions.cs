using MiraAPI.API.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example
{
    [RegisterCustomOptions(ExamplePlugin.Id)]
    public class ExampleOptions : IModOptions
    {
        public LoadableAsset<Sprite> OptionsTabIcon => MiraAssets.Empty;
        public static CustomToggleOption YeOrNa = new CustomToggleOption("Ye or Na", true);
    }
}

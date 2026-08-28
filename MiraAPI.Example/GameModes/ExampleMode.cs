using MiraAPI.GameModes;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.GameModes;

public class ExampleMode : AbstractGameMode
{
    public override string Name => "Example Mode";
    public override string Description => "An example gamemode.";
    public override LoadableAsset<Sprite>? Icon => ExampleAssets.ExampleButton;
    public override Color Color => Color.red;
}

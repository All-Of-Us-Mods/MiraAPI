using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Example;

[RegisterButton]
public class ExampleButton : CustomActionButton
{
    public override string Name => "Example Button";
    public override float Cooldown => 5f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 5;
    public override LoadableAsset<Sprite> Sprite { get; } = new LoadableResourceAsset("MiraAPI.Resources.ExampleButton.png");
    
    protected override void OnClick()
    {
        Logger<ExamplePlugin>.Info("Example button clicked!");
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return true;
    }
}
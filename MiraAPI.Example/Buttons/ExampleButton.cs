using MiraAPI.Example.Modifiers;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Example.Buttons;

[RegisterButton]
public class ExampleButton : CustomActionButton
{
    public override string Name => "Example Button";
    public override float Cooldown => 5f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 5;
    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    protected override void OnClick()
    {
        Logger<ExamplePlugin>.Info("Example button clicked!");
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return PlayerControl.LocalPlayer.HasModifier<GameModifierExample>();
    }
}
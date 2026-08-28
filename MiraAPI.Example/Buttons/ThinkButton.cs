using MiraAPI.Hud;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Buttons;

public class ThinkButton : CustomActionButton
{
    public override string Name => "button.think";
    public override float Cooldown => 15f;
    public override float EffectDuration => 10f;
    public override int MaxUses => 1;
    public override ButtonUsesMode UsesMode => ButtonUsesMode.PerRound;
    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;
    protected override void OnClick()
    {
        Button!.OverrideText("button.think.thinking".Translate());
    }

    public override void OnEffectEnd()
    {
        Button!.OverrideText("button.think".Translate());
    }

    public override bool IsEffectCancellable()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role != null && role.IsImpostor;
    }
}

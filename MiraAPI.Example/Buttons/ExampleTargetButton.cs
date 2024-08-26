using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Buttons;

public class ExampleTargetButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Shoot";
    public override float Cooldown => 0f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;
    
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.CmdCheckMurder(Target);
    }

    public override PlayerControl GetTarget()
    {
        return PlayerControl.LocalPlayer.Data.Role.FindClosestTarget();
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Color.red));
    }

    public override bool IsTargetValid(PlayerControl target)
    {
        return PlayerControl.LocalPlayer.Data.Role.IsValidTarget(target.Data);
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role.IsImpostor;
    }

}
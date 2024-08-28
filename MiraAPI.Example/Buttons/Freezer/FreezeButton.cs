using MiraAPI.Example.Modifiers.Freezer;
using MiraAPI.Example.Roles;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Buttons.Freezer;

[RegisterButton]
public class FreezeButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Freeze";
    public override float Cooldown => 5f;
    public override float EffectDuration => 0f;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;

    protected override void OnClick()
    {
        Target?.RpcAddModifier<FreezeModifier>();
    }

    public override PlayerControl GetTarget()
    {
        return PlayerControl.LocalPlayer.Data.Role.FindClosestTarget();
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.Blue));
    }

    public override bool IsTargetValid(PlayerControl target)
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour role)
    {
        return role is FreezerRole;
    }

}
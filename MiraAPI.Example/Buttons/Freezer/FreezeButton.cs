using MiraAPI.Example.Modifiers.Freezer;
using MiraAPI.Example.Options.Roles;
using MiraAPI.Example.Roles;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Buttons.Freezer;

public class FreezeButton : CustomActionButton<PlayerControl>
{
    public override string Name => "Freeze";

    public override float Cooldown => OptionGroupSingleton<FreezerRoleSettings>.Instance.FreezeDuration;
    public override bool PauseTimerInVent => true;

    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;
    public override MiraKeybind? Keybind => MiraGlobalKeybinds.PrimaryAbility;

    protected override void OnClick()
    {
        Target?.RpcAddModifier<FreezeModifier>();
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Palette.Blue));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is FreezerRole;
    }
}

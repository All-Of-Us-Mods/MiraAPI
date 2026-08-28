using MiraAPI.Example.Roles;
using MiraAPI.MeetingAbilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.MeetingAbilities;

public class EndMeetingButton : MeetingActionButton
{
    public override string Name => "End Meeting";

    public override float Cooldown => 15;

    public override float InitialCooldown => 30;

    public override int MaxUses => 3;

    public override LoadableAsset<Sprite> Sprite => ExampleAssets.ExampleButton;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null && role is MayorRole;
    }

    protected override void OnClick()
    {
        MeetingHud.Instance.RpcClose();
    }
}

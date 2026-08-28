using MiraAPI.MeetingAbilities;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.MeetingAbilities;

public class ShootButton : TargetedMeetingButton
{
    public override string Name => "Shoot";

    public override int MaxUses => 3;

    public override float InitialCooldown => 5;

    public override float Cooldown => 3;

    public override LoadableAsset<Sprite> Sprite => ExampleAssets.SharpshootButton;

    public override Color OutlineColor => Color.red;

    public override bool Enabled(RoleBehaviour r)
    {
        return r.IsImpostor;
    }

    protected override void OnClick(PlayerVoteArea playerVoteArea)
    {
        playerVoteArea.SetDisabled();
        var player = playerVoteArea.GetPlayer();
        if (player != null)
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(player);
        }
    }
}

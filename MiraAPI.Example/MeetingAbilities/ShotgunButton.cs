using MiraAPI.MeetingAbilities;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.MeetingAbilities;

public class ShotgunButton : MultiTargetMeetingButton
{
    public override string Name => "Shotgun";

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
        // Emptu because it only triggers the actual kills once all meeting abilities have been used.
    }

    public override LoadableAsset<Sprite> SpriteActive => ExampleAssets.CallMeetingButton;

    public override int TargetsCount => 4;

    protected override void OnSelect(PlayerVoteArea playerVoteArea, bool toggle)
    {
        HudManager.Instance.PlayerCam.ShakeScreen(0.5f, 2);
    }

    public override void OnFinish()
    {
        foreach (var playerVoteArea in Targets.Keys)
        {
            playerVoteArea.SetDisabled();
            var player = playerVoteArea.GetPlayer();
            if (player != null)
            {
                PlayerControl.LocalPlayer.RpcCustomMurder(player);
            }
        }
    }
}

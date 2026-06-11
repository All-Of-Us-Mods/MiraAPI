using MiraAPI.CustomChats;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.CustomChats;

public class ExampleCustomChat2 : CustomChat
{
    public override string Name => "The chat for the mute";

    public override Color ChatBackgroundColor => Color.gray;

    public override LoadableResourceAsset ChatIcon => ExampleAssets.ExampleButton;

    public override ChatButtonVisualAppearance ChatButtonAppearance => new(ExampleAssets.CallMeetingButton, ExampleAssets.ExampleButton, ExampleAssets.TeleportButton, ExampleAssets.BlueChatBubble);

    public override bool CanSee()
    {
        return true;
    }

    public override bool CanSendMessage()
    {
        return false;
    }
}

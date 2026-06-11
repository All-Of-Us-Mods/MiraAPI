using MiraAPI.CustomChats;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.CustomChats;

public class ExampleCustomChat : CustomChat
{
    public override string Name => "uwu";

    public override Color ChatBackgroundColor => Color.yellow;

    public override LoadableResourceAsset ChatIcon => ExampleAssets.TeleportButton;

    public override bool CanSee()
    {
        return true;
    }
}
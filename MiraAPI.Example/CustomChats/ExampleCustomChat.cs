using MiraAPI.CustomChats;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.CustomChats;

public class ExampleCustomChat : AbstractCustomChat
{
    public override string Name => "Example Chat";

    public override Color ChatBackgroundColor => Color.yellow;

    public override LoadableResourceAsset ChatIcon => ExampleAssets.TeleportButton;

    public override ChatButtonSprites Sprites => new();

    public override bool CanSee()
    {
        return true;
    }
}
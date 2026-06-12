using MiraAPI.CustomChats;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.CustomChats;

/// <summary>
/// The default <see cref="AbstractCustomChat"/>.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class DefaultChat : AbstractCustomChat
{
    public override string Name => "Default Chat";

    public override Color ChatBackgroundColor => Color.white;

    public override LoadableResourceAsset ChatIcon => MiraAssets.DefaultChatIcon;

    public override ChatButtonSprites Sprites => new();

    public override bool CanSee()
    {
        return true;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

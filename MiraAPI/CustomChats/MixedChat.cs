using System.Linq;
using MiraAPI.CustomChats;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.CustomChats;

/// <summary>
/// The default <see cref="AbstractCustomChat"/>.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class MixedChat : AbstractCustomChat
{
    public override string Name => "Mixed Chat";

    public override Color ChatBackgroundColor => Color.blue;

    public override LoadableResourceAsset ChatIcon => MiraAssets.DefaultChatIcon;

    public override ChatButtonSprites Sprites => new();

    public override bool CanSee()
    {
        return CustomChatManager.Chats.Count(x => x.CanSee()) > 1;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

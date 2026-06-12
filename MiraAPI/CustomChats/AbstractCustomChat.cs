using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.CustomChats;

/// <summary>
/// abstract Class for creating custom chats.
/// </summary>
public abstract class AbstractCustomChat
{
    /// <summary>
    /// Gets the name of the custom chat.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the <see cref="Color"/> which the chat background will use.
    /// </summary>
    public abstract Color ChatBackgroundColor { get; }

    /// <summary>
    /// Gets the <see cref="LoadableAsset"/> which is used for the chat icon.
    /// </summary>
    public abstract LoadableAsset<Sprite> ChatIcon { get; }

    /// <summary>
    /// Gets the <see cref="ChatButtonSprites"/> which is used for changing the chat button's visual appearance.
    /// </summary>
    public abstract ChatButtonSprites Sprites { get; }

    /// <summary>
    /// Gets the audio <see cref="LoadableAsset"/> which is played when a message is sent, if null, it will fall back to the default sound.
    /// </summary>
    public virtual LoadableAsset<AudioClip>? MessageSound { get; } = null!;

    /// <summary>
    /// Determines if the local player can view this chat.
    /// </summary>
    /// <returns>True if they can view it, false otherwise.</returns>
    public abstract bool CanSee();

    /// <summary>
    /// Determines if the local player can send messages in this chat.
    /// </summary>
    /// <returns>True if they can send messages, false otherwise.</returns>
    public virtual bool CanSendMessage()
    {
        return CanSee();
    }

    /// <summary>
    /// Callback method for when a message is sent in this custom chat.
    /// </summary>
    /// <param name="sourcePlayer">The <see cref="PlayerControl"/> who sent the message (can be null when a warning message is sent!).</param>
    /// <param name="bubble">the <see cref="ChatBubble"/> instance.</param>
    public virtual void OnMessageSent(PlayerControl? sourcePlayer, ChatBubble bubble)
    {
    }

    /// <summary>
    /// Callback method for when this chat is being opened.
    /// </summary>
    /// <param name="chat">The <see cref="ChatController"/> instance.</param>
    public virtual void OnChatOpen(ChatController chat)
    {
    }

    /// <summary>
    /// Callback method for when this chat is being closed.
    /// </summary>
    /// <param name="chat">The <see cref="ChatController"/> instance.</param>
    public virtual void OnChatClose(ChatController chat)
    {
    }
}

/// <summary>
/// a Class for defining how the chat button looks.
/// </summary>
public class ChatButtonSprites
{
    /// <summary>
    /// Gets the inactive <see cref="LoadableAsset"/> for the Chat Button.
    /// </summary>
    public LoadableAsset<Sprite> InactiveSprite = MiraAssets.NormalChatIdle;

    /// <summary>
    /// Gets the active <see cref="LoadableAsset"/> for the Chat Button.
    /// </summary>
    public LoadableAsset<Sprite> ActiveSprite = MiraAssets.NormalChatHover;

    /// <summary>
    /// Gets the opened <see cref="LoadableAsset"/> for the Chat Button.
    /// </summary>
    public LoadableAsset<Sprite> OpenedSprite = MiraAssets.NormalChatOpen;

    /// <summary>
    /// Gets the notification dot <see cref="LoadableAsset"/> for the Chat Button.
    /// </summary>
    public LoadableAsset<Sprite> NotificationSprite = MiraAssets.ChatNormalBubble;
}

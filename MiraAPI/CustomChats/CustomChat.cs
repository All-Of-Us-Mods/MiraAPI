using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.CustomChats;

/// <summary>
/// abstract Class for creating custom chats.
/// </summary>
public abstract class CustomChat
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
    /// Gets the <see cref="LoadableResourceAsset"/> which is used for the chat icon.
    /// </summary>
    public abstract LoadableResourceAsset ChatIcon { get; }

    /// <summary>
    /// Gets the <see cref="ChatButtonVisualAppearance"/> which is used for changing the chat button's visual appearance.
    /// </summary>
    public virtual ChatButtonVisualAppearance ChatButtonAppearance { get; } =
        ChatButtonVisualAppearance.DefaultAppearance;

    /// <summary>
    /// Gets the <see cref="LoadableAudioResourceAsset"/> which is played when a message is sent, if null, it will fall back to the default sound.
    /// </summary>
    public virtual LoadableAudioResourceAsset? MessageSound { get; } = null!;

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
    /// <param name="sourcePlayer">The <see cref="PlayerControl"/> who sent the message.</param>
    /// <param name="bubble">the <see cref="ChatBubble"/> instance.</param>
    public virtual void OnMessageSent(PlayerControl sourcePlayer, ChatBubble bubble)
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
public class ChatButtonVisualAppearance
{
    /// <summary>
    /// Gets the inactive <see cref="LoadableResourceAsset"/> for the Chat Button.
    /// </summary>
    public LoadableResourceAsset InactiveSprite;

    /// <summary>
    /// Gets the active <see cref="LoadableResourceAsset"/> for the Chat Button.
    /// </summary>
    public LoadableResourceAsset ActiveSprite;

    /// <summary>
    /// Gets the opened <see cref="LoadableResourceAsset"/> for the Chat Button.
    /// </summary>
    public LoadableResourceAsset OpenedSprite;

    /// <summary>
    /// Gets the notification dot <see cref="LoadableResourceAsset"/> for the Chat Button.
    /// </summary>
    public LoadableResourceAsset NotificationSprite;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatButtonVisualAppearance"/> class.
    /// </summary>
    /// <param name="inactiveSprite">The inactive sprite <see cref="LoadableResourceAsset"/>.</param>
    /// <param name="activeSprite">The active sprite <see cref="LoadableResourceAsset"/>.</param>
    /// <param name="openedSprite">The opened sprite <see cref="LoadableResourceAsset"/>.</param>
    /// <param name="notificationSprite">The notification dot sprite <see cref="LoadableResourceAsset"/>.</param>
    public ChatButtonVisualAppearance(LoadableResourceAsset inactiveSprite, LoadableResourceAsset activeSprite, LoadableResourceAsset openedSprite, LoadableResourceAsset notificationSprite)
    {
        InactiveSprite = inactiveSprite;
        ActiveSprite = activeSprite;
        OpenedSprite = openedSprite;
        NotificationSprite = notificationSprite;
    }

    /// <summary>
    /// The default appearance for the Chat Button.
    /// </summary>
    public static readonly ChatButtonVisualAppearance DefaultAppearance = new(
        MiraAssets.NormalChatIdle,
        MiraAssets.NormalChatHover,
        MiraAssets.NormalChatOpen,
        MiraAssets.ChatNormalBubble);
}

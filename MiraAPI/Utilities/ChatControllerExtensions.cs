using System;
using AmongUs.Data;
using MiraAPI.CustomChats;
using MiraAPI.Patches;
using UnityEngine;

namespace MiraAPI.Utilities;

/// <summary>
/// a Class with a bunch of <see cref="ChatController"/> extensions to replace methods related to adding chat messages.
/// </summary>
public static class ChatControllerExtensions
{
    /// <summary>
    /// Adds a chat message to a specific <see cref="AbstractCustomChat"/>.
    /// </summary>
    /// <param name="instance">The <see cref="ChatController"/> Instance.</param>
    /// <param name="sourcePlayer">The <see cref="PlayerControl"/> who sent the message.</param>
    /// <param name="chatText">The message text.</param>
    /// <param name="chat">The target <see cref="AbstractCustomChat"/>.</param>
    /// <param name="censor">Determines if the message should be censored.</param>
    public static void CustomAddChat(this ChatController instance, PlayerControl sourcePlayer, string chatText, AbstractCustomChat chat, bool censor = true)
    {
        if (!sourcePlayer || !PlayerControl.LocalPlayer)
            return;
        if (!ChatControllerCustomChatsPatches.Pages.TryGetValue(chat, out var page)) return;
        NetworkedPlayerInfo data1 = PlayerControl.LocalPlayer.Data;
        NetworkedPlayerInfo data2 = sourcePlayer.Data;
        if (data2 == null || data1 == null || (data2.IsDead && !data1.IsDead))
            return;
        ChatBubble pooledBubble = GetPooledBubble(instance, chat);
        try
        {
            pooledBubble.transform.SetParent(page.transform.GetChild(0));
            pooledBubble.transform.localScale = Vector3.one;
            int num = sourcePlayer == PlayerControl.LocalPlayer ? 1 : 0;
            if (num != 0)
                pooledBubble.SetRight();
            else
                pooledBubble.SetLeft();
            bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
            pooledBubble.SetCosmetics(data2);
            instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2));
            if (censor && DataManager.Settings.Multiplayer.CensorChat)
                chatText = BlockedWords.CensorWords(chatText);
            pooledBubble.SetText(chatText);
            pooledBubble.AlignChildren();
            instance.AlignAllBubbles();
            if (!instance.IsOpenOrOpening && instance.notificationRoutine == null && chat.CanSee())
            {
                instance.chatNotifyDot.sprite = chat.Sprites.NotificationSprite.LoadAsset();
                instance.notificationRoutine = instance.StartCoroutine(instance.BounceDot());
            }
            if (num != 0 || instance.IsOpenOrOpening)
                return;
            if (chat.CanSee())
            {
                var audio = chat.MessageSound != null! ? chat.MessageSound.LoadAsset() : instance.messageSound;
                SoundManager.Instance.PlaySound(audio, false).pitch =
                    (float)(0.5 + sourcePlayer.PlayerId / 15.0);
                instance.chatNotification.SetUp(sourcePlayer, chatText);
            }

            page.activeChildren.Add(pooledBubble);
            chat.OnMessageSent(sourcePlayer, pooledBubble);
        }
        catch (Exception ex)
        {
            ChatController.Logger.Error(ex.ToString());
            page.Reclaim(pooledBubble);
        }
    }

    /// <summary>
    /// Adds a chat note to a specific <see cref="AbstractCustomChat"/>.
    /// </summary>
    /// <param name="instance">The <see cref="ChatController"/> Instance.</param>
    /// <param name="srcPlayer">The <see cref="PlayerControl"/> who sent the message.</param>
    /// <param name="chat">The target <see cref="AbstractCustomChat"/>.</param>
    /// <param name="noteType">The <see cref="ChatNoteTypes"/> of the message.</param>
    public static void CustomAddChatNote(this ChatController instance, NetworkedPlayerInfo srcPlayer, AbstractCustomChat chat, ChatNoteTypes noteType)
    {
        if (!ChatControllerCustomChatsPatches.Pages.TryGetValue(chat, out var page)) return;
        if (srcPlayer == null)
            return;
        ChatBubble pooledBubble = GetPooledBubble(instance, chat);
        pooledBubble.SetCosmetics(srcPlayer);
        pooledBubble.transform.SetParent(page.transform.GetChild(0));
        pooledBubble.transform.localScale = Vector3.one;
        pooledBubble.SetNotification();
        if (noteType == ChatNoteTypes.DidVote)
        {
            int rem = MeetingHud.Instance.GetVotesRemaining();
            pooledBubble.SetName(TranslationController.Instance.GetString(StringNames.MeetingHasVoted, srcPlayer.PlayerName, rem), false, true, Color.green);
        }
        pooledBubble.SetText(string.Empty);
        pooledBubble.AlignChildren();
        instance.AlignAllBubbles();
        if (!instance.IsOpenOrOpening && instance.notificationRoutine == null && chat.CanSee())
        {
            instance.chatNotifyDot.sprite = chat.Sprites.NotificationSprite.LoadAsset();
            instance.notificationRoutine = instance.StartCoroutine(instance.BounceDot());
        }
        if (srcPlayer.Object.AmOwner)
            return;
        if (chat.CanSee())
        {
            var audio = chat.MessageSound != null! ? chat.MessageSound.LoadAsset() : instance.messageSound;
            SoundManager.Instance.PlaySound(audio, false).pitch =
                (float)(0.5 + srcPlayer.PlayerId / 15.0);
        }
        page.activeChildren.Add(pooledBubble);

        chat.OnMessageSent(srcPlayer.Object, pooledBubble);
    }

    /// <summary>
    /// Adds a chat warning to a specific <see cref="AbstractCustomChat"/>.
    /// </summary>
    /// <param name="instance">The <see cref="ChatController"/> Instance.</param>
    /// <param name="warningText">The warning text.</param>
    /// <param name="chat">The target <see cref="AbstractCustomChat"/>.</param>
    public static void CustomAddChatWarning(this ChatController instance, string warningText, AbstractCustomChat chat)
    {
        if (!ChatControllerCustomChatsPatches.Pages.TryGetValue(chat, out var page)) return;
        ChatBubble pooledBubble = GetPooledBubble(instance, chat);
        pooledBubble.transform.SetParent(page.transform.GetChild(0));
        pooledBubble.transform.localScale = Vector3.one;
        pooledBubble.SetRight();
        pooledBubble.SetWarning(warningText);
        pooledBubble.AlignChildren();
        instance.AlignAllBubbles();
        if (!instance.IsOpenOrOpening && instance.notificationRoutine == null && chat.CanSee())
        {
            instance.chatNotifyDot.sprite = chat.Sprites.NotificationSprite.LoadAsset();
            instance.notificationRoutine = instance.StartCoroutine(instance.BounceDot());
        }
        if (chat.CanSee())
        {
            var audio = chat.MessageSound != null! ? chat.MessageSound.LoadAsset() : instance.messageSound;
            SoundManager.Instance.PlaySound(audio, false);
        }
        page.activeChildren.Add(pooledBubble);

        chat.OnMessageSent(null!, pooledBubble);
    }

    /// <summary>
    /// Gets a pooled <see cref="ChatBubble"/> from a specific <see cref="AbstractCustomChat"/>.
    /// </summary>
    /// <param name="instance">The <see cref="ChatController"/> Instance.</param>
    /// <param name="chat">The target <see cref="AbstractCustomChat"/>.</param>
    /// <returns>A pooled <see cref="ChatBubble"/> if the chat is initialized, null otherwise.</returns>
    public static ChatBubble GetPooledBubble(this ChatController instance, AbstractCustomChat chat)
    {
        if (!ChatControllerCustomChatsPatches.Pages.TryGetValue(chat, out var pool)) return null!;
        if (pool.NotInUse == 0)
            pool.ReclaimOldest();
        return pool.Get<ChatBubble>();
    }

    /// <summary>
    /// Switches the currently selected <see cref="AbstractCustomChat"/>.
    /// </summary>
    /// <param name="instance">The <see cref="ChatController"/> Instance.</param>
    /// <param name="chat">The target <see cref="AbstractCustomChat"/>.</param>
    public static void SetChat(this ChatController instance, AbstractCustomChat chat)
    {
        if (ChatControllerCustomChatsPatches.CurrentChat != null!) ChatControllerCustomChatsPatches.CurrentChat.OnChatClose(instance);
        ChatControllerCustomChatsPatches.CurrentChat = chat;
        ChatControllerCustomChatsPatches.Text.text = ChatControllerCustomChatsPatches.CurrentChat.Name;
        ChatControllerCustomChatsPatches.ChatIcon.sprite = ChatControllerCustomChatsPatches.CurrentChat.ChatIcon.LoadAsset();

        instance.StartCoroutine(Effects.ColorFade(instance.backgroundImage, instance.backgroundImage.color, ChatControllerCustomChatsPatches.CurrentChat.ChatBackgroundColor, 0.4f));
        instance.freeChatField.gameObject.SetActive(chat.CanSendMessage());
        instance.quickChatField.gameObject.SetActive(chat.CanSendMessage());

        instance.chatButton.activeSprites.GetComponent<SpriteRenderer>().sprite =
            ChatControllerCustomChatsPatches.CurrentChat.Sprites.ActiveSprite.LoadAsset();
        instance.chatButton.inactiveSprites.GetComponent<SpriteRenderer>().sprite =
            ChatControllerCustomChatsPatches.CurrentChat.Sprites.InactiveSprite.LoadAsset();
        instance.chatButton.selectedSprites.GetComponent<SpriteRenderer>().sprite =
            ChatControllerCustomChatsPatches.CurrentChat.Sprites.OpenedSprite.LoadAsset();
        foreach (var page in ChatControllerCustomChatsPatches.Pages.Values)
        {
            page.gameObject.SetActive(false);
        }

        if (!ChatControllerCustomChatsPatches.Pages.TryGetValue(ChatControllerCustomChatsPatches.CurrentChat, out var pool)) return;
        pool.gameObject.SetActive(true);
        chat.OnChatOpen(instance);
    }
}

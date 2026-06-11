using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using MiraAPI.CustomChats;
using MiraAPI.LocalSettings;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Action = Il2CppSystem.Action;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches;

#pragma warning disable SA1629
/// <summary>
/// Allows players to paste text into chat.
/// Source: https://github.com/CallOfCreator/NewMod/blob/main/NewMod/Patches/ClipboardPatch.cs
/// </summary>
#pragma warning restore SA1629
[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class ChatControllerPastePatch
{
    public static void Prefix(ChatController __instance)
    {
        if (!HudManager.Instance.Chat.IsOpenOrOpening) return;

        var ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (!ctrlPressed || !Input.GetKeyDown(KeyCode.V)) return;

        var clipboard = GUIUtility.systemCopyBuffer;

        if (string.IsNullOrWhiteSpace(clipboard)) return;
        clipboard = clipboard.Replace("<", string.Empty)
            .Replace(">", string.Empty)
            .Replace("\r", string.Empty);

        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            clipboard = clipboard.Replace("\n", string.Empty);

        __instance.freeChatField.textArea.SetText(__instance.freeChatField.textArea.text + clipboard);
    }
}
[HarmonyPatch(typeof(ChatController))]
public static class ChatControllerCustomChatsPatches
{
    private static PassiveButton _previousButton = null!;
    private static PassiveButton _nextButton = null!;
    private static SpriteRenderer _chatIcon = null!;
    private static TextMeshPro _text = null!;
    public static CustomChat CurrentChat = null!;
    private static Dictionary<CustomChat, ObjectPoolBehavior> pages = new();

    [HarmonyPatch(nameof(ChatController.Awake))]
    [HarmonyPostfix]
    public static void ChatController_Awake_Postfix(ChatController __instance)
    {
        pages = new();
        CreatePaginationControls(__instance);
        SetUpObjectPools(__instance);
        SetPage(__instance, CustomChatManager.Chats[0]);
    }
    [HarmonyPatch(nameof(ChatController.AddChatNote))]
    [HarmonyPrefix]
    public static bool ChatController_AddChatNote_Prefix(ChatController __instance, ref NetworkedPlayerInfo srcPlayer, ref ChatNoteTypes noteType)
    {
        __instance.CustomAddChatNote(srcPlayer, CustomChatManager.Chats[0], noteType);
        return false;
    }

    [HarmonyPatch(nameof(ChatController.Update))]
    [HarmonyPrefix]
    public static void ChatController_Update_Prefix(ChatController __instance)
    {
        if (!CurrentChat.CanSee()) SetPage(__instance, CustomChatManager.Chats[0]);

        bool canSend = CurrentChat.CanSendMessage();
        __instance.freeChatField.gameObject.SetActive(DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.FreeChatOrQuickChat && canSend);
        __instance.quickChatField.gameObject.SetActive(DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly && canSend);
    }

    public static ChatBubble GetPooledBubble(ChatController __instance, CustomChat chat)
    {
        if (!pages.TryGetValue(chat, out var pool)) return null;
        if (pool.NotInUse == 0)
            pool.ReclaimOldest();
        return pool.Get<ChatBubble>();
    }

    [HarmonyPatch(nameof(ChatController.AlignAllBubbles))]
    [HarmonyPrefix]
    public static bool ChatController_AlignAllBubbles_Postfix(ChatController __instance)
    {
        foreach (var pool in pages.Values)
        {
            float num1 = 0.0f;
            var activeChildren = pool.activeChildren;
            for (int index = activeChildren.Count - 1; index >= 0; --index)
            {
                var chatBubble = activeChildren[index].TryCast<ChatBubble>();
                if (chatBubble == null) continue;
                float num2 = num1 + chatBubble.Background.size.y;
                Vector3 localPosition = chatBubble.transform.localPosition;
                localPosition.y = num2 - 1.85f;
                chatBubble.transform.localPosition = localPosition;
                num1 = num2 + 0.15f;
            }

            float num3 = -0.3f;
            var scroller = pool.GetComponent<Scroller>();
            scroller.SetYBoundsMin(Mathf.Min(0.0f, -num1 + scroller.Hitbox.bounds.size.y + num3));
        }

        return false;
    }

    private static void CreatePaginationControls(ChatController __instance)
    {
        _text = Object.Instantiate(HudManager.Instance.UseButton.buttonLabelText, __instance.chatScreen.transform);
        _text.color = Color.white;
        _text.alignment = TextAlignmentOptions.MidlineLeft;
        _text.fontSizeMax = 4f;
        _text.fontSizeMin = 2f;
        _text.overflowMode = TextOverflowModes.Overflow;
        _text.transform.localPosition = new Vector3(-6.1f, -0.3f, -490f);
        _text.text = "Default Chat";

        _nextButton = Object.Instantiate(__instance.quickChatButton, __instance.quickChatButton.transform.parent, true);
        _nextButton.transform.localPosition += new Vector3(0, 3, 0);
        _nextButton.name = "UpButton";
        _nextButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButtonChat.LoadAsset();
        _nextButton.GetComponent<BoxCollider2D>().size /= new Vector2(1, 2);
        _nextButton.OnClick = new Button.ButtonClickedEvent();
        var customChats = CustomChatManager.Chats.Where(x => x.CanSee()).ToList();
        _nextButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                int id = customChats.IndexOf(CurrentChat);
                id++;
                if (id > customChats.Count - 1)
                {
                    id = 0;
                }
                var chat = customChats[id];
                SetPage(__instance, chat);
            }));

        _previousButton = Object.Instantiate(_nextButton, __instance.chatScreen.transform, true);
        _previousButton.transform.localPosition -= new Vector3(0, 1, 0);
        _previousButton.name = "LeftArrowButton";
        _previousButton.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = MiraAssets.PreviousButtonChat.LoadAsset();
        _previousButton.OnClick = new Button.ButtonClickedEvent();
        _previousButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                int id = customChats.IndexOf(CurrentChat);
                id--;
                if (id < 0)
                {
                    id = customChats.Count - 1;
                }
                var chat = customChats[id];
                SetPage(__instance, chat);
            }));

        _chatIcon = new GameObject("CurrentChatIcon").AddComponent<SpriteRenderer>();
        _chatIcon.gameObject.transform.SetParent(_nextButton.transform.parent);
        _chatIcon.gameObject.layer = LayerMask.NameToLayer("UI");
        _chatIcon.transform.localPosition = _nextButton.transform.localPosition - new Vector3(0, 0.5f, 0);
        _chatIcon.transform.localScale = Vector3.one / 2f;
    }

    private static void SetUpObjectPools(ChatController __instance)
    {
        foreach (var chat in CustomChatManager.Chats)
        {
            var pool = Object.Instantiate(__instance.chatBubblePool, __instance.chatBubblePool.transform.parent, true);
            pool.GetComponent<Scroller>().active = true;
            pages.Add(chat, pool);
        }
        __instance.chatBubblePool.gameObject.SetActive(false);
    }

    private static void SetPage(ChatController __instance, CustomChat chat)
    {
        if (CurrentChat != null!) CurrentChat.OnChatClose(__instance);
        CurrentChat = chat;
        _text.text = CurrentChat.Name;
        _chatIcon.sprite = CurrentChat.ChatIcon.LoadAsset();

        __instance.StartCoroutine(Effects.ColorFade(__instance.backgroundImage, __instance.backgroundImage.color, CurrentChat.ChatBackgroundColor, 0.4f));
        __instance.freeChatField.gameObject.SetActive(chat.CanSendMessage());
        __instance.quickChatField.gameObject.SetActive(chat.CanSendMessage());

        __instance.chatButton.activeSprites.GetComponent<SpriteRenderer>().sprite =
            CurrentChat.ChatButtonAppearance.ActiveSprite.LoadAsset();
        __instance.chatButton.inactiveSprites.GetComponent<SpriteRenderer>().sprite =
            CurrentChat.ChatButtonAppearance.InactiveSprite.LoadAsset();
        __instance.chatButton.selectedSprites.GetComponent<SpriteRenderer>().sprite =
            CurrentChat.ChatButtonAppearance.OpenedSprite.LoadAsset();
        foreach (var page in pages.Values)
        {
            page.gameObject.SetActive(false);
        }

        if (!pages.TryGetValue(CurrentChat, out var pool)) return;
        pool.gameObject.SetActive(true);
        chat.OnChatOpen(__instance);
    }

    public static void CustomAddChat(this ChatController __instance, PlayerControl sourcePlayer, string chatText, CustomChat chat, bool censor = true)
    {
        if (!sourcePlayer || !PlayerControl.LocalPlayer)
            return;
        if (!pages.TryGetValue(chat, out var page)) return;
        NetworkedPlayerInfo data1 = PlayerControl.LocalPlayer.Data;
        NetworkedPlayerInfo data2 = sourcePlayer.Data;
        if (data2 == null || data1 == null || (data2.IsDead && !data1.IsDead))
            return;
        ChatBubble pooledBubble = GetPooledBubble(__instance, chat);
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
            __instance.SetChatBubbleName(pooledBubble, data2, data2.IsDead, didVote, PlayerNameColor.Get(data2));
            if (censor && DataManager.Settings.Multiplayer.CensorChat)
                chatText = BlockedWords.CensorWords(chatText);
            pooledBubble.SetText(chatText);
            pooledBubble.AlignChildren();
            __instance.AlignAllBubbles();
            if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null && chat.CanSee())
            {
                __instance.chatNotifyDot.sprite = chat.ChatButtonAppearance.NotificationSprite.LoadAsset();
                __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
            }
            if (num != 0 || __instance.IsOpenOrOpening)
                return;
            if (chat.CanSee())
            {
                var audio = chat.MessageSound != null! ? chat.MessageSound.LoadAsset() : __instance.messageSound;
                SoundManager.Instance.PlaySound(audio, false).pitch =
                    (float)(0.5 + sourcePlayer.PlayerId / 15.0);
            }
            __instance.chatNotification.SetUp(sourcePlayer, chatText);

            page.activeChildren.Add(pooledBubble);
            chat.OnMessageSent(sourcePlayer, pooledBubble);
        }
        catch (Exception ex)
        {
            ChatController.Logger.Error(ex.ToString());
            page.Reclaim(pooledBubble);
        }
    }

    public static void CustomAddChatNote(this ChatController __instance, NetworkedPlayerInfo srcPlayer, CustomChat chat, ChatNoteTypes noteType)
    {
        if (!pages.TryGetValue(chat, out var page)) return;
        if (srcPlayer == null)
            return;
        ChatBubble pooledBubble = GetPooledBubble(__instance, chat);
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
        __instance.AlignAllBubbles();
        if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null && chat.CanSee())
        {
            __instance.chatNotifyDot.sprite = chat.ChatButtonAppearance.NotificationSprite.LoadAsset();
            __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
        }
        if (srcPlayer.Object.AmOwner)
            return;
        if (chat.CanSee())
        {
            var audio = chat.MessageSound != null! ? chat.MessageSound.LoadAsset() : __instance.messageSound;
            SoundManager.Instance.PlaySound(audio, false).pitch =
                (float)(0.5 + srcPlayer.PlayerId / 15.0);
        }
        page.activeChildren.Add(pooledBubble);

        chat.OnMessageSent(srcPlayer.Object, pooledBubble);
    }
}

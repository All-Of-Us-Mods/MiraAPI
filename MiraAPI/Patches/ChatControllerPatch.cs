using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using MiraAPI.CustomChats;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
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
    public static PassiveButton PreviousButton { get; private set; } = null!;
    public static PassiveButton NextButton { get; private set; } = null!;
    public static SpriteRenderer ChatIcon { get; private set; } = null!;
    public static TextMeshPro Text { get; private set; } = null!;
    public static AbstractCustomChat CurrentChat { get; set; } = null!;
    public static Dictionary<AbstractCustomChat, ObjectPoolBehavior> Pages { get; private set; } = new();

    [HarmonyPatch(nameof(ChatController.Awake))]
    [HarmonyPostfix]
    public static void ChatController_Awake_Postfix(ChatController __instance)
    {
        Pages = new();
        CreatePaginationControls(__instance);
        SetUpObjectPools(__instance);
        __instance.SetChat(CustomChatManager.Chats[0]);
    }

    [HarmonyPatch(nameof(ChatController.Update))]
    [HarmonyPrefix]
    public static void ChatController_Update_Prefix(ChatController __instance)
    {
        if (!CurrentChat.CanSee()) __instance.SetChat(CustomChatManager.Chats[0]); // Fallback to default chat

        bool canSend = CurrentChat.CanSendMessage();
        __instance.freeChatField.gameObject.SetActive(DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.FreeChatOrQuickChat && canSend);
        __instance.quickChatField.gameObject.SetActive(DataManager.Settings.Multiplayer.ChatMode == QuickChatModes.QuickChatOnly && canSend);
    }

    [HarmonyPatch(nameof(ChatController.AlignAllBubbles))]
    [HarmonyPrefix]
    public static bool ChatController_AlignAllBubbles_Prefix(ChatController __instance)
    {
        foreach (var pool in Pages.Values)
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

    private static void CreatePaginationControls(ChatController instance)
    {
        Text = Object.Instantiate(HudManager.Instance.UseButton.buttonLabelText, instance.chatScreen.transform);
        Text.color = Color.white;
        Text.alignment = TextAlignmentOptions.MidlineLeft;
        Text.fontSizeMax = 4f;
        Text.fontSizeMin = 2f;
        Text.overflowMode = TextOverflowModes.Overflow;
        Text.transform.localPosition = new Vector3(-6.1f, -0.3f, -490f);
        Text.GetComponent<TextTranslatorTMP>().DestroyImmediate();
        Text.text = "Default Chat";

        NextButton = Object.Instantiate(instance.quickChatButton, instance.quickChatButton.transform.parent, true);
        NextButton.transform.localPosition += new Vector3(0, 3, 0);
        NextButton.name = "UpButton";
        NextButton.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButtonChat.LoadAsset();
        NextButton.GetComponent<BoxCollider2D>().size /= new Vector2(1, 2);
        NextButton.OnClick = new Button.ButtonClickedEvent();
        var customChats = CustomChatManager.Chats.Where(x => x.CanSee()).ToList();
        NextButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                int id = customChats.IndexOf(CurrentChat);
                id++;
                if (id > customChats.Count - 1)
                {
                    id = 0;
                }
                var chat = customChats[id];
                instance.SetChat(chat);
            }));

        PreviousButton = Object.Instantiate(NextButton, instance.chatScreen.transform, true);
        PreviousButton.transform.localPosition -= new Vector3(0, 1, 0);
        PreviousButton.name = "LeftArrowButton";
        PreviousButton.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = MiraAssets.PreviousButtonChat.LoadAsset();
        PreviousButton.OnClick = new Button.ButtonClickedEvent();
        PreviousButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                int id = customChats.IndexOf(CurrentChat);
                id--;
                if (id < 0)
                {
                    id = customChats.Count - 1;
                }
                var chat = customChats[id];
                instance.SetChat(chat);
            }));

        ChatIcon = new GameObject("CurrentChatIcon").AddComponent<SpriteRenderer>();
        ChatIcon.gameObject.transform.SetParent(NextButton.transform.parent);
        ChatIcon.gameObject.layer = LayerMask.NameToLayer("UI");
        ChatIcon.transform.localPosition = NextButton.transform.localPosition - new Vector3(0, 0.5f, 10);
        ChatIcon.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
    }

    private static void SetUpObjectPools(ChatController __instance)
    {
        foreach (var chat in CustomChatManager.Chats)
        {
            var pool = Object.Instantiate(__instance.chatBubblePool, __instance.chatBubblePool.transform.parent, true);
            pool.GetComponent<Scroller>().active = true;
            Pages.Add(chat, pool);
        }
        __instance.chatBubblePool.gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(ChatController.AddChat))]
    [HarmonyPrefix]
    public static bool ChatController_AddChat_Prefix(ChatController __instance, ref PlayerControl sourcePlayer, ref string chatText, ref bool censor)
    {
        __instance.CustomAddChat(sourcePlayer, chatText, CustomChatManager.Chats[0], censor);
        return false;
    }

    [HarmonyPatch(nameof(ChatController.AddChatNote))]
    [HarmonyPrefix]
    public static bool ChatController_AddChatNote_Prefix(ChatController __instance, ref NetworkedPlayerInfo srcPlayer, ref ChatNoteTypes noteType)
    {
        __instance.CustomAddChatNote(srcPlayer, CustomChatManager.Chats[0], noteType);
        return false;
    }
}

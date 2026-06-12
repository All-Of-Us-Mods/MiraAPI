using System;
using System.Collections;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using BepInEx.Unity.IL2CPP.Utils;
using MiraAPI.CustomChats;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Patches;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Networking;

/// <summary>
/// Custom SendChat RPC adapted for custom chats.
/// </summary>
public static class CustomSendChatRpc
{
    [MethodRpc((uint)MiraRpc.SendChat, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcCustomSendChat(
        this PlayerControl source,
        string chatText,
        int id)
    {
        AbstractCustomChat chat = CustomChatManager.Chats[id];
        if (!chat.CanSee()) return;

        HudManager.Instance.Chat.CustomAddChat(source, chatText, chat);
    }
}
